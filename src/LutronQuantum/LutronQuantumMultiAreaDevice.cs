using System;
using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace LutronQuantum
{
	/// <summary>
	/// Multi-area Lutron Quantum device. Owns the single connection to the QSE-CI-NWK-E and fans
	/// responses out to independently bridgeable child devices.
	/// </summary>
	/// <remarks>
	/// A QSE-CI-NWK-E offers one integration connection, so one program has to own the port. This
	/// device is that owner: it inherits the transport, login, monitoring and polling of
	/// <see cref="LutronQuantumDevice"/> but carries no area of its own. Instead it builds a child
	/// for each entry in the config's three dictionaries:
	///
	///   areas        -> {parentKey}-areas-{key}    lighting scenes and raise/lower  (#AREA)
	///   shades       -> {parentKey}-shades-{key}   shade groups                     (#SHADEGRP)
	///   buttonGroups -> {parentKey}-buttons-{key}  emulated keypad buttons          (#DEVICE)
	///
	/// Splitting them this way is what lets a room's lighting and its shades sit on the same EISC
	/// bridge at different join starts, or on separate bridges for separate programs, over one
	/// shared connection.
	/// </remarks>
	public class LutronQuantumMultiAreaDevice : LutronQuantumDevice
	{
		private readonly List<LutronQuantumAreaDevice> _areas = new List<LutronQuantumAreaDevice>();
		private readonly List<LutronQuantumShadeAreaDevice> _shadeAreas = new List<LutronQuantumShadeAreaDevice>();
		private readonly List<LutronQuantumButtonGroupDevice> _buttonGroups = new List<LutronQuantumButtonGroupDevice>();

		private Dictionary<string, LutronQuantumAreaDevice> _areasByAreaId =
			new Dictionary<string, LutronQuantumAreaDevice>();

		/// <summary>
		/// Lighting area children.
		/// </summary>
		public IList<LutronQuantumAreaDevice> Areas { get { return _areas; } }

		/// <summary>
		/// Shade group children.
		/// </summary>
		public IList<LutronQuantumShadeAreaDevice> ShadeAreas { get { return _shadeAreas; } }

		/// <summary>
		/// Button group children.
		/// </summary>
		public IList<LutronQuantumButtonGroupDevice> ButtonGroups { get { return _buttonGroups; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="deviceConfig">Device configuration</param>
		/// <param name="propsConfig">Multi-area properties configuration</param>
		/// <param name="comms">Comms for the shared connection</param>
		public LutronQuantumMultiAreaDevice(DeviceConfig deviceConfig, LutronQuantumMultiAreaPropertiesConfig propsConfig,
			IBasicCommunication comms)
			: base(deviceConfig, propsConfig, comms)
		{
			BuildAreas(propsConfig);
			BuildShadeAreas(propsConfig);
			BuildButtonGroups(propsConfig);

			if (_areas.Count == 0 && _shadeAreas.Count == 0 && _buttonGroups.Count == 0)
			{
				Debug.LogInformation(this, "No areas, shades or button groups configured; this device will own comms only");
			}
		}

		private void BuildAreas(LutronQuantumMultiAreaPropertiesConfig propsConfig)
		{
			if (propsConfig.Areas == null) return;

			foreach (var entry in propsConfig.Areas)
			{
				if (entry.Value == null)
				{
					Debug.LogInformation(this, "Area '{0}' has no configuration and was skipped", entry.Key);
					continue;
				}

				if (string.IsNullOrEmpty(entry.Value.AreaId))
				{
					Debug.LogInformation(this, "Area '{0}' has no areaId; it will build but cannot recall scenes until one is set",
						entry.Key);
				}

				var key = string.Format("{0}-areas-{1}", Key, entry.Key);
				var device = new LutronQuantumAreaDevice(key, BuildName(entry.Value.Name, entry.Key), entry.Key, entry.Value, this);

				_areas.Add(device);
				DeviceManager.AddDevice(device);

				Debug.LogDebug(this, "Built area '{0}' as '{1}' (areaId '{2}', {3} scenes)",
					entry.Key, key, entry.Value.AreaId, device.LightingScenes.Count);
			}

			ReindexAreas();
		}

		private void BuildShadeAreas(LutronQuantumMultiAreaPropertiesConfig propsConfig)
		{
			if (propsConfig.Shades == null) return;

			foreach (var entry in propsConfig.Shades)
			{
				if (entry.Value == null)
				{
					Debug.LogInformation(this, "Shade area '{0}' has no configuration and was skipped", entry.Key);
					continue;
				}

				var key = string.Format("{0}-shades-{1}", Key, entry.Key);
				var device = new LutronQuantumShadeAreaDevice(key, BuildName(entry.Value.Name, entry.Key), entry.Key, entry.Value, this);

				var unaddressable = device.ShadeGroups.Count(g => string.IsNullOrEmpty(g.Id));
				if (unaddressable > 0)
				{
					Debug.LogInformation(this,
						"Shade area '{0}' has {1} group(s) with no id. Shade groups must exist in the Lutron program to be addressable; where they do not, drive the shades as keypad buttons instead",
						entry.Key, unaddressable);
				}

				_shadeAreas.Add(device);
				DeviceManager.AddDevice(device);

				Debug.LogDebug(this, "Built shade area '{0}' as '{1}' ({2} groups)", entry.Key, key, device.ShadeGroups.Count);
			}
		}

		private void BuildButtonGroups(LutronQuantumMultiAreaPropertiesConfig propsConfig)
		{
			if (propsConfig.ButtonGroups == null) return;

			foreach (var entry in propsConfig.ButtonGroups)
			{
				if (entry.Value == null)
				{
					Debug.LogInformation(this, "Button group '{0}' has no configuration and was skipped", entry.Key);
					continue;
				}

				var key = string.Format("{0}-buttons-{1}", Key, entry.Key);
				var device = new LutronQuantumButtonGroupDevice(key, BuildName(entry.Value.Name, entry.Key), entry.Key, entry.Value, this);

				foreach (var button in device.Buttons.Where(b => !b.IsValid))
				{
					Debug.LogInformation(this,
						"Button group '{0}' button '{1}' has no target with both a deviceId and a buttonId and cannot be driven",
						entry.Key, button.Name);
				}

				_buttonGroups.Add(device);
				DeviceManager.AddDevice(device);

				Debug.LogDebug(this, "Built button group '{0}' as '{1}' ({2} buttons)", entry.Key, key, device.Buttons.Count);
			}
		}

		private string BuildName(string configured, string fallbackKey)
		{
			return string.IsNullOrEmpty(configured) ? string.Format("{0} {1}", Name, fallbackKey) : configured;
		}

		/// <summary>
		/// Rebuilds the area ID lookup used to route responses. Called after construction and
		/// whenever an area's ID is changed at runtime.
		/// </summary>
		public void ReindexAreas()
		{
			var index = new Dictionary<string, LutronQuantumAreaDevice>();

			foreach (var area in _areas.Where(a => !string.IsNullOrEmpty(a.AreaId)))
			{
				if (index.ContainsKey(area.AreaId))
				{
					Debug.LogInformation(this, "Areas '{0}' and '{1}' share areaId '{2}'; responses will route to '{0}'",
						index[area.AreaId].AreaKey, area.AreaKey, area.AreaId);
					continue;
				}

				index.Add(area.AreaId, area);
			}

			_areasByAreaId = index;
		}

		/// <summary>
		/// Polls every configured area. Shades and buttons report nothing pollable.
		/// </summary>
		public override void Poll()
		{
			if (_areas.Count == 0)
			{
				base.Poll();
				return;
			}

			foreach (var area in _areas)
			{
				area.Poll();
			}
		}

		/// <summary>
		/// Routes a response to the children it belongs to, falling back to the base implementation.
		/// </summary>
		/// <param name="response">Delimited response from the device</param>
		protected override void ProcessResponse(string response)
		{
			if (string.IsNullOrEmpty(response) || !response.Contains(','))
			{
				base.ProcessResponse(response);
				return;
			}

			try
			{
				var data = response.Split(',');
				if (data.Length < 2)
				{
					base.ProcessResponse(response);
					return;
				}

				switch (data[0].ToLower())
				{
					case "~area":
						{
							LutronQuantumAreaDevice area;
							if (_areasByAreaId.TryGetValue(data[1], out area))
							{
								area.ProcessAreaResponse(data);
								return;
							}

							Debug.LogVerbose(this, "No area configured for area ID '{0}'", data[1]);
							return;
						}

					case "~device":
						{
							// a keypad can serve more than one room, so every group gets a look
							foreach (var group in _buttonGroups)
							{
								group.ProcessDeviceResponse(data);
							}

							// still let the base dispatch to any ILutronDevice children (QSE-IO etc.)
							base.ProcessResponse(response);
							return;
						}
				}

				base.ProcessResponse(response);
			}
			catch (Exception ex)
			{
				Debug.LogDebug(this, "ProcessResponse Exception Message: {0}", ex.Message);
				Debug.LogVerbose(this, "ProcessResponse Exception Stack Trace: {0}", ex.StackTrace);
			}
		}
	}
}
