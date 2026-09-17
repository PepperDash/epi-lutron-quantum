using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Plugins
{
	/// <summary>
	/// One room's shade groups, bridged independently of the comms device that owns the serial port.
	/// </summary>
	/// <remarks>
	/// Drives <c>#SHADEGRP</c>, which requires shade groups to exist in the Lutron program — they
	/// appear in the integration report's Shade Group table. Where that table is empty the shades are
	/// only reachable as keypad buttons; use a button group instead.
	///
	/// Shades are fire and forget: the system reports no position back, so this device publishes no
	/// shade feedback beyond the group names and the shared connection's online status.
	/// </remarks>
	public class LutronQuantumShadeAreaDevice : EssentialsBridgeableDevice
	{
		private const string CommsSet = "#";

		private readonly LutronQuantumMultiAreaDevice _parent;
		private readonly List<LutronQuantumShadeGroupConfig> _shadeGroups;

		/// <summary>
		/// The configuration key this device was built from.
		/// </summary>
		public string ShadeAreaKey { get; private set; }

		/// <summary>
		/// Shade groups in this room, in join order.
		/// </summary>
		public IList<LutronQuantumShadeGroupConfig> ShadeGroups { get { return _shadeGroups; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">Device key, conventionally {parentKey}-shades-{shadeAreaKey}</param>
		/// <param name="name">Device name</param>
		/// <param name="shadeAreaKey">The configuration key for this room</param>
		/// <param name="config">Shade area configuration</param>
		/// <param name="parent">The comms device that owns the port</param>
		public LutronQuantumShadeAreaDevice(string key, string name, string shadeAreaKey,
			LutronQuantumShadeAreaConfig config, LutronQuantumMultiAreaDevice parent)
			: base(key, name)
		{
			if (config == null) throw new ArgumentNullException("config");
			if (parent == null) throw new ArgumentNullException("parent");

			_parent = parent;

			ShadeAreaKey = shadeAreaKey;
			_shadeGroups = config.ShadeGroups ?? new List<LutronQuantumShadeGroupConfig>();
		}

		/// <summary>
		/// Sends text through the parent's connection.
		/// </summary>
		/// <param name="text">Text to send</param>
		public void SendText(string text)
		{
			_parent.SendText(text);
		}

		#region Overrides of EssentialsBridgeableDevice

		/// <summary>
		/// Links the shade groups to the EISC bridge.
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinStart"></param>
		/// <param name="joinMapKey"></param>
		/// <param name="bridge"></param>
		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new LutronQuantumShadeAreaJoinMap(joinStart);

			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}

			var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
			if (customJoins != null)
			{
				joinMap.SetCustomJoinData(customJoins);
			}

			_parent.OnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

			trilist.SetSigTrueAction(joinMap.AllRaise.JoinNumber, () => AllRaise());
			trilist.SetSigTrueAction(joinMap.AllLower.JoinNumber, () => AllLower());
			trilist.SetSigTrueAction(joinMap.AllStop.JoinNumber, () => AllStop());

			var span = joinMap.ShadeGroupRaise.JoinSpan;

			if (_shadeGroups.Count > span)
			{
				Debug.LogInformation(this, "{0} shade groups configured but only {1} join sets are available; the remainder are ignored",
					_shadeGroups.Count, span);
			}

			for (var i = 0; i < _shadeGroups.Count && i < span; i++)
			{
				// capture per iteration so the closures below bind to the right group
				var id = _shadeGroups[i].Id;
				var offset = (uint)i;

				trilist.SetSigTrueAction(joinMap.ShadeGroupRaise.JoinNumber + offset, () => ShadeGroupRaise(id));
				trilist.SetSigTrueAction(joinMap.ShadeGroupLower.JoinNumber + offset, () => ShadeGroupLower(id));
				trilist.SetSigTrueAction(joinMap.ShadeGroupStop.JoinNumber + offset, () => ShadeGroupStop(id));
			}

			UpdateBridgeFeedbacks(trilist, joinMap);

			trilist.OnlineStatusChange += (sender, args) =>
			{
				if (!args.DeviceOnLine) return;

				UpdateBridgeFeedbacks(trilist, joinMap);
			};
		}

		private void UpdateBridgeFeedbacks(BasicTriList trilist, LutronQuantumShadeAreaJoinMap joinMap)
		{
			trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

			_parent.OnlineFeedback.FireUpdate();

			var span = joinMap.ShadeGroupName.JoinSpan;
			for (var i = 0; i < _shadeGroups.Count && i < span; i++)
			{
				var group = _shadeGroups[i];
				var label = string.IsNullOrEmpty(group.Name) ? group.Id : group.Name;

				trilist.SetString(joinMap.ShadeGroupName.JoinNumber + (uint)i, label);
			}
		}

		#endregion

		#region Shade control

		/// <summary>
		/// Raises one shade group
		/// </summary>
		/// <param name="id">Shade group integration ID</param>
		public void ShadeGroupRaise(string id)
		{
			SendShadeGroupAction(id, ELutronAction.Raise);
		}

		/// <summary>
		/// Lowers one shade group
		/// </summary>
		/// <param name="id">Shade group integration ID</param>
		public void ShadeGroupLower(string id)
		{
			SendShadeGroupAction(id, ELutronAction.Lower);
		}

		/// <summary>
		/// Stops one shade group
		/// </summary>
		/// <param name="id">Shade group integration ID</param>
		public void ShadeGroupStop(string id)
		{
			SendShadeGroupAction(id, ELutronAction.Stop);
		}

		/// <summary>
		/// Raises every configured shade group
		/// </summary>
		public void AllRaise()
		{
			SendToAll(ELutronAction.Raise);
		}

		/// <summary>
		/// Lowers every configured shade group
		/// </summary>
		public void AllLower()
		{
			SendToAll(ELutronAction.Lower);
		}

		/// <summary>
		/// Stops every configured shade group
		/// </summary>
		public void AllStop()
		{
			SendToAll(ELutronAction.Stop);
		}

		private void SendToAll(ELutronAction action)
		{
			foreach (var group in _shadeGroups.Where(g => !string.IsNullOrEmpty(g.Id)))
			{
				SendShadeGroupAction(group.Id, action);
			}
		}

		private void SendShadeGroupAction(string id, ELutronAction action)
		{
			if (string.IsNullOrEmpty(id))
			{
				Debug.LogDebug(this, "Shade group ID is null or empty, verify configuration");
				return;
			}

			SendText(string.Format("{0}SHADEGRP,{1},{2}", CommsSet, id, (int)action));
		}

		#endregion
	}
}
