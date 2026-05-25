using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Lighting;

namespace LutronQuantum
{
	public class LutronQuantumPropertiesConfig
	{
		/// <summary>
		/// JSON control object
		/// </summary>		
		[JsonProperty("control")]
		public EssentialsControlPropertiesConfig Control { get; set; }

		/// <summary>
		/// Username
		/// </summary>
		[JsonProperty("username")]
		public string Username { get; set; }

		/// <summary>
		/// Password
		/// </summary>
		[JsonProperty("password")]
		public string Password { get; set; }

		/// <summary>
		/// Serializes the poll time value
		/// </summary>
		[JsonProperty("pollTimeMs")]
		public long? PollTimeMs { get; set; }

		/// <summary>
		/// Serializes the warning timeout value
		/// </summary>
		[JsonProperty("warningTimeoutMs")]
		public long? WarningTimeoutMs { get; set; }

		/// <summary>
		/// Serializes the error timeout value
		/// </summary>
		[JsonProperty("errorTimeoutMs")]
		public long? ErrorTimeoutMs { get; set; }

		/// <summary>
		/// Integration ID
		/// </summary>
		[JsonProperty("integrationId")]
		public string IntegrationId { get; set; }

		/// <summary>
		/// Shade Group 1 ID
		/// </summary>
		[JsonProperty("shadeGroup1Id")]
		public string ShadeGroup1Id { get; set; }

		/// <summary>
		/// Shade Group 2 ID
		/// </summary>
		[JsonProperty("shadeGroup2Id")]
		public string ShadeGroup2Id { get; set; }

		/// <summary>
		/// Scenes
		/// </summary>
		[JsonProperty("scenes")]
		public List<LightingScene> Scenes { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public LutronQuantumPropertiesConfig()
		{
			Scenes = new List<LightingScene>();
		}
	}
}