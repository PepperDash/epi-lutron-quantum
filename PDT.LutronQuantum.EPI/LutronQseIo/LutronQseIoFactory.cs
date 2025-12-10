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
			MinimumEssentialsFrameworkVersion = "1.10.4";

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
				Debug.Console(2, new string('*', 80));
				Debug.Console(2, new string('*', 80));
				Debug.Console(0, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);				
				
				// get the plugin device properties configuration object & check for null 
				var propertiesConfig = dc.Properties.ToObject<LutronQseIoPropertiesConfig>();
				if (propertiesConfig != null) return new LutronQseIoDevice(dc, propertiesConfig);
				Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
				return null;
			}
			catch (Exception ex)
			{
				Debug.Console(0, "[{0}] Factory BuildDevice Exception: {1}", dc.Key, ex);
				return null;
			}
		}
	}
}