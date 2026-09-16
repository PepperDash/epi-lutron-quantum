using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Plugins
{
	/// <summary>
	/// Configuration for one room's shade groups.
	/// </summary>
	public class LutronQuantumShadeAreaConfig
	{
		/// <summary>
		/// Display name for the child device. Defaults to the parent name plus the key.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// Shade groups in this room, in the order they are assigned to joins.
		/// </summary>
		[JsonProperty("shadeGroups")]
		public List<LutronQuantumShadeGroupConfig> ShadeGroups { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public LutronQuantumShadeAreaConfig()
		{
			ShadeGroups = new List<LutronQuantumShadeGroupConfig>();
		}
	}
}
