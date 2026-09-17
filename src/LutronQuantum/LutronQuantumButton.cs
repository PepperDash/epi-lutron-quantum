using System;
using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugins
{
	/// <summary>
	/// A bridgeable button that drives one or more real keypad buttons with <c>#DEVICE</c>.
	/// </summary>
	/// <remarks>
	/// Where the Lutron program exposes shades as separate per-wall buttons with no group object,
	/// listing several targets recreates that grouping here: one join sends consecutive commands to
	/// every target. Targets routinely span more than one keypad, so each carries its own
	/// <c>deviceId</c>.
	/// </remarks>
	public class LutronQuantumButton
	{
		private const string CommsSet = "#";

		private readonly LutronQuantumButtonGroupDevice _parent;
		private readonly LutronQuantumButtonConfig _config;
		private readonly List<LutronQuantumButtonTarget> _targets;

		private bool _isPressed;

		/// <summary>
		/// Display name, or the first target's button ID when no name is configured.
		/// </summary>
		public string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(_config.Name)) return _config.Name;

				return _targets.Count > 0 ? _targets[0].ButtonId : string.Empty;
			}
		}

		/// <summary>
		/// Configured behaviour, defaulting to <see cref="EButtonAction.PressAndRelease"/>.
		/// </summary>
		public EButtonAction ButtonAction
		{
			get { return _config.ButtonAction ?? EButtonAction.PressAndRelease; }
		}

		/// <summary>
		/// Configured feedback mode, defaulting to <see cref="EButtonFeedbackMode.Echo"/>.
		/// </summary>
		public EButtonFeedbackMode FeedbackMode
		{
			get { return _config.FeedbackMode ?? EButtonFeedbackMode.Echo; }
		}

		/// <summary>
		/// Sort order used to assign the button to a bridge join.
		/// </summary>
		public int SortOrder { get { return _config.SortOrder; } }

		/// <summary>
		/// The keypad buttons this button drives.
		/// </summary>
		public IList<LutronQuantumButtonTarget> Targets { get { return _targets; } }

		/// <summary>
		/// Pressed feedback, reported according to <see cref="FeedbackMode"/>.
		/// </summary>
		public BoolFeedback IsPressedFeedback { get; private set; }

		/// <summary>
		/// True when at least one target can be driven.
		/// </summary>
		public bool IsValid { get { return _targets.Any(t => t.IsValid); } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="config">Button configuration</param>
		/// <param name="parent">Owning button group, used for comms and the default keypad ID</param>
		public LutronQuantumButton(LutronQuantumButtonConfig config, LutronQuantumButtonGroupDevice parent)
		{
			if (config == null) throw new ArgumentNullException("config");
			if (parent == null) throw new ArgumentNullException("parent");

			_config = config;
			_parent = parent;

			_targets = (config.Targets ?? new List<LutronQuantumButtonTargetConfig>())
				.Select(t => new LutronQuantumButtonTarget(t, parent.DeviceId))
				.ToList();

			IsPressedFeedback = new BoolFeedback("IsPressedFeedback", () => _isPressed);
		}

		/// <summary>
		/// Drives the button from a bridge join.
		/// </summary>
		/// <param name="pressed">
		/// The state of the driving join. For <see cref="EButtonAction.PressAndHold"/> every target
		/// is held while this is true and released when it goes false; every other action fires once
		/// on the rising edge and ignores the falling edge.
		/// </param>
		public void Press(bool pressed)
		{
			if (!IsValid)
			{
				Debug.LogDebug(_parent, "Button '{0}' has no target with both a deviceId and a buttonId, verify configuration", Name);
				return;
			}

			switch (ButtonAction)
			{
				case EButtonAction.PressAndHold:
					SendToTargets(pressed ? ELutronDeviceAction.Press : ELutronDeviceAction.Release);
					if (FeedbackMode == EButtonFeedbackMode.Echo) SetIsPressed(pressed);
					break;

				case EButtonAction.Release:
					if (!pressed) return;
					SendToTargets(ELutronDeviceAction.Release);
					if (FeedbackMode == EButtonFeedbackMode.Echo) PulseIsPressed();
					break;

				case EButtonAction.MultiTap:
					if (!pressed) return;
					SendToTargets(ELutronDeviceAction.DoubleTap);
					if (FeedbackMode == EButtonFeedbackMode.Echo) PulseIsPressed();
					break;

				default:
					if (!pressed) return;
					SendToTargets(ELutronDeviceAction.Press);
					SendToTargets(ELutronDeviceAction.Release);
					if (FeedbackMode == EButtonFeedbackMode.Echo) PulseIsPressed();
					break;
			}
		}

		/// <summary>
		/// Updates a target's reported state from a <c>~DEVICE</c> report and recalculates feedback.
		/// </summary>
		/// <param name="deviceId">Reported keypad integration ID</param>
		/// <param name="buttonId">Reported component number</param>
		/// <param name="action">Reported action number</param>
		/// <returns>True when this button owns the reported target</returns>
		public bool ProcessReportedAction(string deviceId, string buttonId, ELutronDeviceAction action)
		{
			var matched = false;

			foreach (var target in _targets.Where(t => t.DeviceId == deviceId && t.ButtonId == buttonId))
			{
				matched = true;

				switch (action)
				{
					case ELutronDeviceAction.Press:
					case ELutronDeviceAction.Hold:
						target.IsPressed = true;
						break;

					case ELutronDeviceAction.Release:
					case ELutronDeviceAction.HoldRelease:
						target.IsPressed = false;
						break;
				}
			}

			// in Echo mode the join we drove is the only truth, so reports never move feedback
			if (matched && FeedbackMode != EButtonFeedbackMode.Echo)
			{
				RecalculateFeedback();
			}

			return matched;
		}

		private void RecalculateFeedback()
		{
			// only targets we could actually address can ever report, so ignore the rest —
			// otherwise an unconfigured target would hold All mode low forever
			var reporting = _targets.Where(t => t.IsValid).ToList();
			if (reporting.Count == 0) return;

			SetIsPressed(FeedbackMode == EButtonFeedbackMode.All
				? reporting.All(t => t.IsPressed)
				: reporting.Any(t => t.IsPressed));
		}

		private void SendToTargets(ELutronDeviceAction action)
		{
			foreach (var target in _targets.Where(t => t.IsValid))
			{
				_parent.SendText(string.Format("{0}DEVICE,{1},{2},{3}", CommsSet, target.DeviceId, target.ButtonId, (int)action));
			}
		}

		private void SetIsPressed(bool value)
		{
			if (_isPressed == value) return;

			_isPressed = value;
			IsPressedFeedback.FireUpdate();
		}

		// a tap has no falling edge of its own, so show the press and immediately clear it
		private void PulseIsPressed()
		{
			SetIsPressed(true);
			SetIsPressed(false);
		}
	}

	/// <summary>
	/// One keypad button driven by a <see cref="LutronQuantumButton"/>.
	/// </summary>
	public class LutronQuantumButtonTarget
	{
		private readonly LutronQuantumButtonTargetConfig _config;
		private readonly string _deviceId;

		/// <summary>
		/// Integration ID of the keypad, falling back to the group's default.
		/// </summary>
		public string DeviceId { get { return _deviceId; } }

		/// <summary>
		/// Component (button) number on the keypad.
		/// </summary>
		public string ButtonId { get { return _config.ButtonId; } }

		/// <summary>
		/// Last state reported for this target by a <c>~DEVICE</c> message.
		/// </summary>
		public bool IsPressed { get; set; }

		/// <summary>
		/// True when the target has both a keypad ID and a component number.
		/// </summary>
		public bool IsValid
		{
			get { return !string.IsNullOrEmpty(_deviceId) && !string.IsNullOrEmpty(_config.ButtonId); }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="config">Target configuration</param>
		/// <param name="groupDeviceId">Default keypad ID from the owning group</param>
		public LutronQuantumButtonTarget(LutronQuantumButtonTargetConfig config, string groupDeviceId)
		{
			if (config == null) throw new ArgumentNullException("config");

			_config = config;
			_deviceId = string.IsNullOrEmpty(config.DeviceId) ? groupDeviceId : config.DeviceId;
		}
	}
}
