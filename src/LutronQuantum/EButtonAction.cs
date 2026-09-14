namespace LutronQuantum
{
	/// <summary>
	/// Configured behaviour of an emulated keypad button.
	/// </summary>
	/// <remarks>
	/// Names match the Lutron LEAP plugin's button actions so configuration reads the same across
	/// both plugins, but each maps to a <see cref="ELutronDeviceAction"/> on the wire.
	/// </remarks>
	public enum EButtonAction
	{
		/// <summary>
		/// Momentary tap — Press then Release are sent together on the rising edge. Suits
		/// single-action and toggle buttons (scene recall). This is the default.
		/// </summary>
		PressAndRelease,

		/// <summary>
		/// Press is sent on the rising edge and Release on the falling edge, so the button stays
		/// held for as long as the bridge join is held. Required for shade and master raise/lower
		/// buttons, where release is what stops travel.
		/// </summary>
		PressAndHold,

		/// <summary>
		/// Release only, sent on the rising edge.
		/// </summary>
		Release,

		/// <summary>
		/// Double-tap, sent on the rising edge.
		/// </summary>
		MultiTap
	}
}
