using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Lighting;
using LightingBase = PepperDash.Essentials.Devices.Common.Lighting.LightingBase;

namespace LutronQuantum
{
	/// <summary>
	/// One lighting area of a multi-area Lutron Quantum system, bridged independently of the comms
	/// device that owns the serial port.
	/// </summary>
	/// <remarks>
	/// Lighting only — scene recall and master raise/lower, both via <c>#AREA</c>. A room's shades
	/// and keypad buttons are separate child devices, so each can be bridged on its own join range.
	/// </remarks>
	public class LutronQuantumAreaDevice : LightingBase
	{
		private const string CommsSet = "#";
		private const string CommsGet = "?";

		private readonly LutronQuantumMultiAreaDevice _parent;

		/// <summary>
		/// The area's integration ID.
		/// </summary>
		public string AreaId { get; private set; }

		/// <summary>
		/// The configuration key this area was built from.
		/// </summary>
		public string AreaKey { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">Device key, conventionally {parentKey}-areas-{areaKey}</param>
		/// <param name="name">Device name</param>
		/// <param name="areaKey">The configuration key for this area</param>
		/// <param name="config">Area configuration</param>
		/// <param name="parent">The comms device that owns the port</param>
		public LutronQuantumAreaDevice(string key, string name, string areaKey, LutronQuantumAreaConfig config,
			LutronQuantumMultiAreaDevice parent)
			: base(key, name)
		{
			if (config == null) throw new ArgumentNullException("config");
			if (parent == null) throw new ArgumentNullException("parent");

			_parent = parent;

			AreaKey = areaKey;
			AreaId = config.AreaId;

			LightingScenes = config.Scenes ?? new List<LightingScene>();
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
		/// Links the area to the EISC bridge.
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinStart"></param>
		/// <param name="joinMapKey"></param>
		/// <param name="bridge"></param>
		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new LutronQuantumAreaJoinMap(joinStart);

			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}

			var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
			if (customJoins != null)
			{
				joinMap.SetCustomJoinData(customJoins);
			}

			LinkScenesToApi(trilist, joinMap);

			// online and comms health belong to the shared connection, so report the parent's state
			// to every area that depends on it
			_parent.OnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
			_parent.CommunicationMonitorFeedback.LinkInputSig(trilist.UShortInput[joinMap.CommunicationMonitorStatus.JoinNumber]);
			if (_parent.SocketStatusFeedback != null)
				_parent.SocketStatusFeedback.LinkInputSig(trilist.UShortInput[joinMap.SocketStatus.JoinNumber]);

			trilist.SetStringSigAction(joinMap.AreaIdSet.JoinNumber, SetAreaId);

			trilist.SetBoolSigAction(joinMap.Raise.JoinNumber, b =>
			{
				if (b)
					MasterRaise();
				else
					MasterRaiseLowerStop();
			});

			trilist.SetBoolSigAction(joinMap.Lower.JoinNumber, b =>
			{
				if (b)
					MasterLower();
				else
					MasterRaiseLowerStop();
			});

			UpdateBridgeFeedbacks(trilist, joinMap);

			trilist.OnlineStatusChange += (sender, args) =>
			{
				if (!args.DeviceOnLine) return;

				UpdateBridgeFeedbacks(trilist, joinMap);
			};
		}

		/// <summary>
		/// Wires the scene joins.
		/// </summary>
		/// <remarks>
		/// Does what the framework's LinkLightingToApi does, with two corrections: the select-by-index
		/// join is driven as the analog it actually is, and the index is range checked — the framework
		/// indexes the scene list directly, so a value past the end throws.
		/// </remarks>
		private void LinkScenesToApi(BasicTriList trilist, LutronQuantumAreaJoinMap joinMap)
		{
			trilist.SetUShortSigAction(joinMap.SelectSceneByIndex.JoinNumber, index =>
			{
				if (index >= LightingScenes.Count)
				{
					Debug.LogDebug(this, "Scene index {0} is out of range; {1} scene(s) configured", index, LightingScenes.Count);
					return;
				}

				SelectScene(LightingScenes[index]);
			});

			var span = joinMap.SelectSceneDirect.JoinSpan;

			if (LightingScenes.Count > span)
			{
				Debug.LogInformation(this, "{0} scenes configured but only {1} scene joins are available; the remainder are ignored",
					LightingScenes.Count, span);
			}

			for (var i = 0; i < LightingScenes.Count && i < span; i++)
			{
				// capture per iteration so the closure binds to the right scene
				var index = i;
				var scene = LightingScenes[index];

				trilist.SetSigTrueAction(joinMap.SelectSceneDirect.JoinNumber + (uint)index, () => SelectScene(scene));
				scene.IsActiveFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SelectSceneDirect.JoinNumber + (uint)index]);
			}
		}

		private void UpdateBridgeFeedbacks(BasicTriList trilist, LutronQuantumAreaJoinMap joinMap)
		{
			trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

			_parent.OnlineFeedback.FireUpdate();
			_parent.CommunicationMonitorFeedback.FireUpdate();
			if (_parent.SocketStatusFeedback != null)
				_parent.SocketStatusFeedback.FireUpdate();

			var span = joinMap.SelectSceneDirect.JoinSpan;
			for (var i = 0; i < LightingScenes.Count && i < span; i++)
			{
				var scene = LightingScenes[i];

				trilist.SetString(joinMap.SelectSceneDirect.JoinNumber + (uint)i, scene.Name);
				trilist.SetBool(joinMap.ButtonVisibility.JoinNumber + (uint)i, true);
				scene.IsActiveFeedback.FireUpdate();
			}
		}

		#endregion

		#region Area control

		/// <summary>
		/// Scene select
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"SelectScene", "params":[{LightingScene scene}]}
		/// </example>
		public override void SelectScene(LightingScene scene)
		{
			if (scene == null) return;

			if (string.IsNullOrEmpty(AreaId))
			{
				Debug.LogDebug(this, "SelectScene: Area ID ('{0}') is null or empty, verify configuration", AreaId);
				return;
			}

			SendText(string.Format("{0}AREA,{1},{2},{3}", CommsSet, AreaId, (int)ELutronAction.Scene, scene.ID));

			Poll();
		}

		/// <summary>
		/// Begins raising the lights in the area
		/// </summary>
		public void MasterRaise()
		{
			SendAreaAction(ELutronAction.Raise);
		}

		/// <summary>
		/// Begins lowering the lights in the area
		/// </summary>
		public void MasterLower()
		{
			SendAreaAction(ELutronAction.Lower);
		}

		/// <summary>
		/// Stops a raise or lower in progress
		/// </summary>
		public void MasterRaiseLowerStop()
		{
			SendAreaAction(ELutronAction.Stop);
		}

		/// <summary>
		/// Polls the area's scene status
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"Poll", "params":[]}
		/// </example>
		public void Poll()
		{
			if (string.IsNullOrEmpty(AreaId)) return;

			SendText(string.Format("{0}AREA,{1},{2}", CommsGet, AreaId, (int)ELutronAction.Scene));
		}

		/// <summary>
		/// Sets the area integration ID at runtime
		/// </summary>
		/// <param name="id">Area integration ID</param>
		public void SetAreaId(string id)
		{
			if (string.IsNullOrEmpty(id)) return;

			AreaId = id;

			// the parent routes responses by area ID, so its index has to follow
			_parent.ReindexAreas();

			Poll();
		}

		private void SendAreaAction(ELutronAction action)
		{
			if (string.IsNullOrEmpty(AreaId))
			{
				Debug.LogDebug(this, "Area ID ('{0}') is null or empty, verify configuration", AreaId);
				return;
			}

			SendText(string.Format("{0}AREA,{1},{2}", CommsSet, AreaId, (int)action));
		}

		#endregion

		#region Response handling

		/// <summary>
		/// Handles an ~AREA report addressed to this area's integration ID.
		/// </summary>
		/// <param name="data">Comma split response</param>
		public void ProcessAreaResponse(string[] data)
		{
			if (data == null || data.Length < 4) return;

			int action;
			if (!Int32.TryParse(data[2], out action)) return;
			if (action != (int)ELutronAction.Scene) return;

			CurrentLightingScene = LightingScenes.FirstOrDefault(s => s.ID.Equals(data[3]));
			OnLightingSceneChange();
		}

		#endregion
	}
}
