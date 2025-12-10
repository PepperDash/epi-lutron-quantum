using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;


namespace LutronQuantum
{
	public class LutronQseIoDevice : EssentialsBridgeableDevice, ILutronDevice
    {
		private LutronQuantumDevice _parentDevice;
		private readonly LutronQseIoPropertiesConfig _propertiesConfig;
		
		public Dictionary<string, BoolWithFeedback> Feedbacks = new Dictionary<string, BoolWithFeedback>();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="deviceConfig"></param>
		/// <param name="propertiesConfig"></param>
		public LutronQseIoDevice(DeviceConfig deviceConfig, LutronQseIoPropertiesConfig propertiesConfig)
			: base(deviceConfig.Key, deviceConfig.Name)
		{			
			_propertiesConfig = propertiesConfig;

			Feedbacks.Add("1", new BoolWithFeedback());
			Feedbacks.Add("2", new BoolWithFeedback());
			Feedbacks.Add("3", new BoolWithFeedback());
			Feedbacks.Add("4", new BoolWithFeedback());
			Feedbacks.Add("5", new BoolWithFeedback());

		}

		/// <summary>
		/// Custom activate device
		/// </summary>
		/// <returns></returns>
		public override bool CustomActivate()
		{
			_parentDevice = DeviceManager.GetDeviceForKey(_propertiesConfig.LightingDeviceKey) as LutronQuantumDevice;
			if (_parentDevice == null)
			{
				Debug.Console(0, this, "LutronQuantumDevice device {0} does not exist", _propertiesConfig.LightingDeviceKey);
			}
			else
			{
				_parentDevice.AddDevice(_propertiesConfig.IntegrationId, this);
			}

			AddPostActivationAction(Initialize);
						
			return true;
		}

		/// <summary>
		/// Links the plugin device to the EISC bridge
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinStart"></param>
		/// <param name="joinMapKey"></param>
		/// <param name="bridge"></param>
		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new LutronQseIoBridgeJoinMap(joinStart);

			// This adds the join map to the collection on the bridge
			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}

			var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
			if (customJoins != null)
			{
				joinMap.SetCustomJoinData(customJoins);
			}

			Debug.Console(TraceLevel, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(TraceLevel, "Linking to Bridge Type {0}", GetType().Name);

			// link joins to bridge
			trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

			foreach (var item in Feedbacks)
			{
				var join = Int32.Parse(item.Key) + joinMap.ContactClosure.JoinNumber - 1;
				Debug.Console(VerboseLevel, this, "Linking key-{0} join-{1}", item.Key, join);
				item.Value.Feedback.LinkInputSig(trilist.BooleanInput[(uint) @join]);
			}
		}


		#region ILutronDevice members

		/// <summary>
		/// Device initialization
		/// </summary>
		public void DeviceInitialize()
		{
			CrestronInvoke.BeginInvoke(o =>
			{
				var rnd = new Random();

				Thread.Sleep(rnd.Next(10000));				
				_parentDevice.SendText("#MONITORING,2,1");
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("~DEVICE,{0},1", _propertiesConfig.IntegrationId));
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("~DEVICE,{0},2", _propertiesConfig.IntegrationId));
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("~DEVICE,{0},3", _propertiesConfig.IntegrationId));
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("~DEVICE,{0},4", _propertiesConfig.IntegrationId));
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("~DEVICE,{0},5", _propertiesConfig.IntegrationId));
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("?DEVICE,{0},1,35", _propertiesConfig.IntegrationId));
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("?DEVICE,{0},2,35", _propertiesConfig.IntegrationId));
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("?DEVICE,{0},3,35", _propertiesConfig.IntegrationId));
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("?DEVICE,{0},4,35", _propertiesConfig.IntegrationId));
				Thread.Sleep(rnd.Next(1000, 5000));
				_parentDevice.SendText(string.Format("?DEVICE,{0},5,35", _propertiesConfig.IntegrationId));
			});
		}

		/// <summary>
		/// Process response 
		/// </summary>
		/// <param name="message"></param>
		/// <remarks>
		/// (NWK pdf pg.161) QSE-IO
		/// Get the state of the contact closure inputs (CCIs)		
		/// </remarks>
		/// <example>
		/// ~DEVICE,{integrationId},{componentNumber},{actionNumber}
		/// ~DEVICE,QSMID,1,3\r\n	// 3 == occupied/button press
		/// ~DEVICE,QSMID,5,4\r\n	// 4 == unoccupied/button release
		/// </example>
		public void ProcessResponse(string[] message)
		{
			Debug.Console(DebugLevel, this, "ProcessResponse: {0},{1},{2},{3}", message[0], message[1], message[2], message[3]);

			BoolWithFeedback fb;
			if (!Feedbacks.TryGetValue(message[2], out fb)) return;

			// closed/occupied
			if (message[3] == "3")
			{
				fb.Value = false;
			}
			// open/unoccuppied
			if (message[3] == "4")
			{
				fb.Value = true;
			}
		}

		#endregion

		#region DebugLevels

		/// <summary>
		/// Trace level (0)
		/// </summary>
		public uint TraceLevel { get; set; }

		/// <summary>
		/// Debug level (1)
		/// </summary>
		public uint DebugLevel { get; set; }

		/// <summary>
		/// Verbose Level (2)
		/// </summary>        
		public uint VerboseLevel { get; set; }

		/// <summary>
		/// Resets debug levels for this device instancee
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"ResetDebugLevels", "params":[]}
		/// </example>
		public void ResetDebugLevels()
		{
			TraceLevel = 0;
			DebugLevel = 1;
			VerboseLevel = 2;
		}

		/// <summary>
		/// Sets the debug levels for this device instance
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"SetDebugLevels", "params":[{level, 0-2}]}
		/// </example>
		/// <param name="level"></param>
		public void SetDebugLevels(uint level)
		{
			TraceLevel = level;
			DebugLevel = level;
			VerboseLevel = level;
		}

		#endregion
	}
}