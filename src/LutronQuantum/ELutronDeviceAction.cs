namespace PepperDash.Essentials.Plugins
{
	/// <summary>
	/// Action numbers for the DEVICE command.
	/// </summary>
	/// <remarks>
	/// Lutron integration protocol, "DEVICE Command-specific fields — Action Numbers and Parameters".
	/// Raise/lower style buttons (shades, master raise/lower) require Press followed by Release;
	/// a single-action or toggle button only requires Press.
	/// </remarks>
	public enum ELutronDeviceAction : int
	{
		Enable = 1,
		Disable = 2,
		Press = 3,
		Release = 4,
		Hold = 5,
		DoubleTap = 6,
		HoldRelease = 32
	}
}
