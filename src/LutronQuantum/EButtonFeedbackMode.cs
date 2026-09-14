namespace LutronQuantum
{
	/// <summary>
	/// How a button reports pressed feedback when it drives more than one keypad button.
	/// </summary>
	/// <remarks>
	/// A grouped button exists because Lutron did not group the underlying buttons for us, so there
	/// is no single object in the Lutron system whose state we can mirror. These modes choose what
	/// "pressed" means for the group.
	/// </remarks>
	public enum EButtonFeedbackMode
	{
		/// <summary>
		/// Feedback echoes the join we are driving and ignores what the system reports back. Nothing
		/// depends on the Lutron program reporting button state, so this always works — it just does
		/// not reflect someone pressing the physical keypad. This is the default.
		/// </summary>
		Echo,

		/// <summary>
		/// Feedback is high only while <b>every</b> target is reporting pressed.
		/// </summary>
		All,

		/// <summary>
		/// Feedback is high while <b>any</b> target is reporting pressed.
		/// </summary>
		Any
	}
}
