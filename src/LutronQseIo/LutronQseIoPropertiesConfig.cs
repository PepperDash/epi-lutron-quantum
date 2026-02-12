using Newtonsoft.Json;

namespace LutronQuantum
{
	public class LutronQseIoPropertiesConfig
	{
		[JsonProperty("integrationId")]
		public string IntegrationId { get; set; }

		[JsonProperty("lightingDeviceKey")]
		public string LightingDeviceKey { get; set; }
	}	
}