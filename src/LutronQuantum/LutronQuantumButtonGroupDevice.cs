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
	/// One room's emulated keypad buttons, bridged independently of the comms device that owns the
	/// serial port.
	/// </summary>
	/// <remarks>
	/// Drives <c>#DEVICE</c> against keypad integration IDs. This is the path to anything the Lutron
	/// program exposes only as a keypad button — shades and blackouts where no shade group object
	/// exists, glass frost, and so on. A single configured button can drive several real buttons at
	/// once, across more than one keypad.
	/// </remarks>
	public class LutronQuantumButtonGroupDevice : EssentialsBridgeableDevice
	{
		private readonly LutronQuantumMultiAreaDevice _parent;
		private readonly List<LutronQuantumButton> _buttons;

		/// <summary>
		/// The configuration key this device was built from.
		/// </summary>
		public string ButtonGroupKey { get; private set; }

		/// <summary>
		/// Default keypad integration ID for targets that do not specify one.
		/// </summary>
		public string DeviceId { get; private set; }

		/// <summary>
		/// Buttons in this room, in sort order.
		/// </summary>
		public IList<LutronQuantumButton> Buttons { get { return _buttons; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">Device key, conventionally {parentKey}-buttons-{buttonGroupKey}</param>
		/// <param name="name">Device name</param>
		/// <param name="buttonGroupKey">The configuration key for this room</param>
		/// <param name="config">Button group configuration</param>
		/// <param name="parent">The comms device that owns the port</param>
		public LutronQuantumButtonGroupDevice(string key, string name, string buttonGroupKey,
			LutronQuantumButtonGroupConfig config, LutronQuantumMultiAreaDevice parent)
			: base(key, name)
		{
			if (config == null) throw new ArgumentNullException("config");
			if (parent == null) throw new ArgumentNullException("parent");

			_parent = parent;

			ButtonGroupKey = buttonGroupKey;
			DeviceId = config.DeviceId;

			_buttons = (config.Buttons ?? new List<LutronQuantumButtonConfig>())
				.OrderBy(b => b.SortOrder)
				.Select(b => new LutronQuantumButton(b, this))
				.ToList();
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
		/// Links the buttons to the EISC bridge.
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinStart"></param>
		/// <param name="joinMapKey"></param>
		/// <param name="bridge"></param>
		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new LutronQuantumButtonGroupJoinMap(joinStart);

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

			var span = joinMap.ButtonPress.JoinSpan;

			if (_buttons.Count > span)
			{
				Debug.LogInformation(this, "{0} buttons configured but only {1} button joins are available; the remainder are ignored",
					_buttons.Count, span);
			}

			for (var i = 0; i < _buttons.Count && i < span; i++)
			{
				var button = _buttons[i];

				// one digital drives both edges, so a PressAndHold button stays held for as long as
				// SIMPL holds the join
				trilist.SetBoolSigAction(joinMap.ButtonPress.JoinNumber + (uint)i, button.Press);

				button.IsPressedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.ButtonPress.JoinNumber + (uint)i]);
			}

			UpdateBridgeFeedbacks(trilist, joinMap);

			trilist.OnlineStatusChange += (sender, args) =>
			{
				if (!args.DeviceOnLine) return;

				UpdateBridgeFeedbacks(trilist, joinMap);
			};
		}

		private void UpdateBridgeFeedbacks(BasicTriList trilist, LutronQuantumButtonGroupJoinMap joinMap)
		{
			trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

			_parent.OnlineFeedback.FireUpdate();

			var span = joinMap.ButtonName.JoinSpan;
			for (var i = 0; i < _buttons.Count && i < span; i++)
			{
				trilist.SetString(joinMap.ButtonName.JoinNumber + (uint)i, _buttons[i].Name);
				_buttons[i].IsPressedFeedback.FireUpdate();
			}
		}

		#endregion

		#region Direct control

		/// <summary>
		/// Taps a button by name, as though its bridge join were pressed and released.
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"PressButton", "params":["Shades All Up"]}
		/// </example>
		/// <param name="name">Button name, matched case-insensitively</param>
		public void PressButton(string name)
		{
			var button = FindButton(name);
			if (button == null) return;

			button.Press(true);
			button.Press(false);
		}

		/// <summary>
		/// Holds or releases a button by name. Use this rather than
		/// <see cref="PressButton"/> to drive a PressAndHold button, which needs the two edges
		/// separated to move and then stop shade travel.
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"SetButton", "params":["Shades All Up", true]}
		/// </example>
		/// <param name="name">Button name, matched case-insensitively</param>
		/// <param name="pressed">True to press or hold, false to release</param>
		public void SetButton(string name, bool pressed)
		{
			var button = FindButton(name);
			if (button == null) return;

			button.Press(pressed);
		}

		/// <summary>
		/// Logs the configured buttons with the join each one occupies and the targets it drives.
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"ListButtons", "params":[]}
		/// </example>
		public void ListButtons()
		{
			Debug.LogInformation(this, "{0} button(s), default deviceId '{1}'", _buttons.Count, DeviceId);

			for (var i = 0; i < _buttons.Count; i++)
			{
				var button = _buttons[i];
				var targets = string.Join(" ", button.Targets
					.Select(t => string.Format("{0}/{1}", t.DeviceId ?? "?", t.ButtonId ?? "?"))
					.ToArray());

				Debug.LogInformation(this, "  join +{0}  '{1}'  {2}  {3}  -> {4}",
					i, button.Name, button.ButtonAction, button.FeedbackMode, targets);
			}
		}

		private LutronQuantumButton FindButton(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				Debug.LogInformation(this, "Button name is null or empty");
				return null;
			}

			var button = _buttons.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));
			if (button == null)
			{
				Debug.LogInformation(this, "No button named '{0}'; call ListButtons to see what is configured", name);
			}

			return button;
		}

		#endregion

		#region Response handling

		/// <summary>
		/// Offers a ~DEVICE report to every button, so those configured for All or Any feedback can
		/// follow the physical keypad.
		/// </summary>
		/// <param name="data">Comma split response</param>
		public void ProcessDeviceResponse(string[] data)
		{
			if (data == null || data.Length < 4) return;

			int action;
			if (!Int32.TryParse(data[3], out action)) return;

			foreach (var button in _buttons)
			{
				button.ProcessReportedAction(data[1], data[2], (ELutronDeviceAction)action);
			}
		}

		#endregion
	}
}
