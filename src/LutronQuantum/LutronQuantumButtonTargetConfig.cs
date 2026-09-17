using Newtonsoft.Json;

namespace PepperDash.Essentials.Plugins
{
	/// <summary>
	/// One keypad button driven by a configured button.
	/// </summary>
	/// <remarks>
	/// Addressed as <c>#DEVICE,{deviceId},{buttonId},{action}</c>. Both values come from the
	/// integration report's device table, where <c>deviceId</c> is the keypad's own integration ID —
	/// a different number from the area's. A single grouped button routinely spans more than one
	/// keypad, which is why the ID lives here rather than only on the group.
	/// </remarks>
	public class LutronQuantumButtonTargetConfig
	{
		/// <summary>
		/// Optional label for this target, used in logs.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// Integration ID of the keypad. Optional — falls back to the group's <c>deviceId</c>.
		/// </summary>
		[JsonProperty("deviceId")]
		public string DeviceId { get; set; }

		/// <summary>
		/// Component (button) number on the keypad.
		/// </summary>
		[JsonProperty("buttonId")]
		public string ButtonId { get; set; }
	}
}
