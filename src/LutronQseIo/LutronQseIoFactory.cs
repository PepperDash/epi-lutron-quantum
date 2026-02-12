using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace LutronQuantum
{
	public class LutronQseIoFactory : EssentialsPluginDeviceFactory<LutronQseIoDevice>
	{
		/// <summary>
		/// Plugin device factory constructor
		/// </summary>
		public LutronQseIoFactory()
		{
			// Set the minimum Essentials Framework Version
			MinimumEssentialsFrameworkVersion = "2.12.1";

			// In the constructor we initialize the list with the typenames that will build an instance of this device
			// only include unique typenames, when the constructur is used all the typenames will be evaluated in lower case.
			TypeNames = new List<string> { "LutronQseIo" };
		}

		/// <summary>
		/// Builds and returns an instance of EnceliumXDevice
		/// </summary>
		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			try
			{
				Debug.LogVerbose(new string('*', 80));
				Debug.LogVerbose(new string('*', 80));
				Debug.LogInformation("[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);				
				
				// get the plugin device properties configuration object & check for null 
				var propertiesConfig = dc.Properties.ToObject<LutronQseIoPropertiesConfig>();
				if (propertiesConfig != null) return new LutronQseIoDevice(dc, propertiesConfig);
				Debug.LogInformation("[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
				return null;
			}
			catch (Exception ex)
			{
				Debug.LogInformation("[{0}] Factory BuildDevice Exception: {1}", dc.Key, ex);
				return null;
			}
		}
	}
}