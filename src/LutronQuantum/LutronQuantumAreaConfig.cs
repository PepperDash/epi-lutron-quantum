using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Lighting;

namespace LutronQuantum
{
	/// <summary>
	/// Configuration for one lighting area — scene recall and master raise/lower.
	/// </summary>
	/// <remarks>
	/// Areas cover lighting only. Shades live in the properties' <c>shades</c> dictionary and keypad
	/// buttons in <c>buttonGroups</c>, so each can be bridged on its own join range.
	/// </remarks>
	public class LutronQuantumAreaConfig
	{
		/// <summary>
		/// Display name for the area's child device. Defaults to the parent name plus the area key.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The area's integration ID, from the integration report's Area table.
		/// </summary>
		[JsonProperty("areaId")]
		public string AreaId { get; set; }

		/// <summary>
		/// Scenes available in this area, recalled with <c>#AREA</c>.
		/// </summary>
		[JsonProperty("scenes")]
		public List<LightingScene> Scenes { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public LutronQuantumAreaConfig()
		{
			Scenes = new List<LightingScene>();
		}
	}
}
