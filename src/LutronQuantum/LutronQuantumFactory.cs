using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace LutronQuantum
{
	public class LutronQuantumFactory : EssentialsPluginDeviceFactory<LutronQuantumDevice>
	{
		/// <summary>
		/// Plugin device factory constructor
		/// </summary>
		public LutronQuantumFactory()
		{
			// Set the minimum Essentials Framework Version
			MinimumEssentialsFrameworkVersion = "2.12.1";

			// In the constructor we initialize the list with the typenames that will build an instance of this device
			// only include unique typenames, when the constructur is used all the typenames will be evaluated in lower case.
			TypeNames = new List<string> { "lutronQuantum", "lutronQs" };
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
				var propertiesConfig = dc.Properties.ToObject<LutronQuantumPropertiesConfig>();
				if (propertiesConfig == null)
				{
					Debug.LogInformation("[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
					return null;
				}				

				// build the plugin device comms (for all other comms methods) & check for null			
				var comms = CommFactory.CreateCommForDevice(dc);
				if (comms != null)
				{
					var useDeviceCommands = dc.Type.Equals("lutronQs", StringComparison.OrdinalIgnoreCase);
					return new LutronQuantumDevice(dc, propertiesConfig, comms, useDeviceCommands);
				}
				Debug.LogInformation("[{0}] Factory: failed to create comm for {1}", dc.Key, dc.Name);
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