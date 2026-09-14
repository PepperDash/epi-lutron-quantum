using Newtonsoft.Json;

namespace LutronQuantum
{
	/// <summary>
	/// Configuration for one shade group, driven with <c>#SHADEGRP</c>.
	/// </summary>
	/// <remarks>
	/// Shade groups have to exist in the Lutron program to be addressable — they appear in the
	/// integration report's Shade Group table. Where that table is empty the shades are only
	/// reachable as keypad buttons, so use <see cref="LutronQuantumButtonGroupConfig"/> instead.
	/// </remarks>
	public class LutronQuantumShadeGroupConfig
	{
		/// <summary>
		/// Display name, reported on the shade group's serial feedback join.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The shade group's integration ID.
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }
	}
}
