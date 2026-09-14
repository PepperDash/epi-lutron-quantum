using System.Collections.Generic;
using Newtonsoft.Json;

namespace LutronQuantum
{
	/// <summary>
	/// Configuration for one room's bridgeable keypad buttons.
	/// </summary>
	public class LutronQuantumButtonGroupConfig
	{
		/// <summary>
		/// Display name for the child device. Defaults to the parent name plus the key.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// Default keypad integration ID for targets that do not specify one.
		/// </summary>
		[JsonProperty("deviceId")]
		public string DeviceId { get; set; }

		/// <summary>
		/// Buttons exposed for this room.
		/// </summary>
		[JsonProperty("buttons")]
		public List<LutronQuantumButtonConfig> Buttons { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public LutronQuantumButtonGroupConfig()
		{
			Buttons = new List<LutronQuantumButtonConfig>();
		}
	}
}
