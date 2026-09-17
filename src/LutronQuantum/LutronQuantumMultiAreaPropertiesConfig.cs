using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Plugins
{
	/// <summary>
	/// Properties configuration for the <c>lutronQuantumMultiArea</c> device type.
	/// </summary>
	/// <remarks>
	/// Extends the single-area properties so the transport, credentials and monitoring settings are
	/// configured exactly as they are for <c>lutronQuantum</c>. The top level owns comms only.
	/// Lighting, shades and buttons are three sibling dictionaries, each keyed by room, and every
	/// entry becomes its own bridgeable child device — so a room's lighting and its shades can sit
	/// on the same EISC bridge at different join starts, or on different bridges entirely.
	/// </remarks>
	public class LutronQuantumMultiAreaPropertiesConfig : LutronQuantumPropertiesConfig
	{
		/// <summary>
		/// Lighting areas keyed by room. Children are built as <c>{parentKey}-areas-{key}</c>.
		/// </summary>
		[JsonProperty("areas")]
		public Dictionary<string, LutronQuantumAreaConfig> Areas { get; set; }

		/// <summary>
		/// Shade groups keyed by room. Children are built as <c>{parentKey}-shades-{key}</c>.
		/// </summary>
		[JsonProperty("shades")]
		public Dictionary<string, LutronQuantumShadeAreaConfig> Shades { get; set; }

		/// <summary>
		/// Keypad buttons keyed by room. Children are built as <c>{parentKey}-buttons-{key}</c>.
		/// </summary>
		[JsonProperty("buttonGroups")]
		public Dictionary<string, LutronQuantumButtonGroupConfig> ButtonGroups { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public LutronQuantumMultiAreaPropertiesConfig()
		{
			Areas = new Dictionary<string, LutronQuantumAreaConfig>();
			Shades = new Dictionary<string, LutronQuantumShadeAreaConfig>();
			ButtonGroups = new Dictionary<string, LutronQuantumButtonGroupConfig>();
		}
	}
}
