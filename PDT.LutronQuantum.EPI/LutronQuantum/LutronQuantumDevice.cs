using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Lighting;
using PepperDash.Essentials.Core.Queues;

namespace LutronQuantum
{
	public class LutronQuantumDevice : LightingBase
	{
		private readonly DeviceConfig _deviceConfig;

		private readonly IBasicCommunication _comms;
		private readonly GenericCommunicationMonitor _commsMonitor;
		private readonly GenericQueue _commsRxQueue;

		private const string CommsDelimiter = "\r\n";
		private readonly bool _commsIsRs232;

		private const string CommsSet = "#";
		private const string CommsGet = "?";

		private CTimer _subscribeAfterLogin;

		public StatusMonitorBase CommunicationMonitor { get { return _commsMonitor; } }
		public BoolFeedback OnlineFeedback { get; private set; }
		public IntFeedback CommunicationMonitorFeedback { get; private set; }
		public IntFeedback SocketStatusFeedback { get; private set; }

		public string IntegrationId;
		public string ShadeGroup1Id;
		public string ShadeGroup2Id;

		public string Username;
		public string Password;

		public Dictionary<string, ILutronDevice> LutronDevices = new Dictionary<string, ILutronDevice>();

		public LutronQuantumDevice(DeviceConfig deviceConfig, LutronQuantumPropertiesConfig propsConfig, IBasicCommunication comms)
			: base(deviceConfig.Key, deviceConfig.Name)
		{
			_deviceConfig = deviceConfig;

			Debug.Console(TraceLevel, this, "Constructing new {0} instance", _deviceConfig.Name);

			ResetDebugLevels();

			IntegrationId = propsConfig.IntegrationId;
			ShadeGroup1Id = propsConfig.ShadeGroup1Id;
			ShadeGroup2Id = propsConfig.ShadeGroup2Id;

			Username = string.IsNullOrEmpty(propsConfig.Username) 
				? propsConfig.Control.TcpSshProperties.Username 
				: propsConfig.Username;

			Password = string.IsNullOrEmpty(propsConfig.Password)
				? propsConfig.Control.TcpSshProperties.Password
				: propsConfig.Password;

			if (LightingScenes == null) LightingScenes = new List<LightingScene>();
			LightingScenes = propsConfig.Scenes;

			_comms = comms;
			_commsIsRs232 = propsConfig.Control.Method == eControlMethod.Com;

			var pollTime = propsConfig.PollTimeMs ?? 60000;
			var warningTimeoutMs = propsConfig.WarningTimeoutMs ?? 180000;
			var errorTimeoutMs = propsConfig.ErrorTimeoutMs ?? 300000;

			_commsMonitor = new GenericCommunicationMonitor(this, _comms, pollTime, warningTimeoutMs, errorTimeoutMs, Poll);
			_commsMonitor.StatusChange += OnCommunicationMonitorStatusChange;
			_commsRxQueue = new GenericQueue(deviceConfig.Key + "-queue");

			OnlineFeedback = _commsMonitor.IsOnlineFeedback;
			CommunicationMonitorFeedback = new IntFeedback(() => (int)_commsMonitor.Status);

			// needed to check for username/password prompts
			_comms.TextReceived += OnTextReceived;

			var commsGather = new CommunicationGather(_comms, CommsDelimiter);
			commsGather.LineReceived += OnLineRecieved;

			var socket = _comms as ISocketStatus;
			if (socket != null)
			{
				socket.ConnectionChange += OnSocketConnectionChange;
				SocketStatusFeedback = new IntFeedback(() => (int)socket.ClientStatus);
			}

			Debug.Console(TraceLevel, this, "Constructing new {0} instance complete", _deviceConfig.Name);
			Debug.Console(TraceLevel, new string('*', 80));
			Debug.Console(TraceLevel, new string('*', 80));
		}

		/// <summary>
		/// Initialize
		/// </summary>
		public override void Initialize()
		{
			_comms.Connect();
			_commsMonitor.StatusChange += (sender, args) => Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Notice, "Communication monitor state: {0}; message: {1}",
				args.Status, args.Message);
			_commsMonitor.Start();

			if (_commsIsRs232) SubscribeToFeedback();

			base.Initialize();
		}

		private void OnCommunicationMonitorStatusChange(object sender, MonitorStatusChangeEventArgs args)
		{
			Debug.Console(DebugLevel, this, "Communication Status: ({0}) {1}, {2}", args.Status, args.Status.ToString(), args.Message);
		}

		private void OnSocketConnectionChange(object sender, GenericSocketStatusChageEventArgs args)
		{
			Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Notice, "Socket Status: ({0}) {1}",
						args.Client.ClientStatus, args.Client.ClientStatus.ToString());

			//var telnetNegotation = new byte[] { 0xFF, 0xFE, 0x01, 0xFF, 0xFE, 0x21, 0xFF, 0xFC, 0x01, 0xFF, 0xFC, 0x03 };
			//if (args.Client.IsConnected)
			//{
			//	args.Client.SendBytes(telnetNegotation);
			//}
		}

		#region Overrides of EssentialsBridgeableDevice

		/// <summary>
		/// Links the plugin device to the EISC bridge
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinStart"></param>
		/// <param name="joinMapKey"></param>
		/// <param name="bridge"></param>
		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new LutronQuantumBridgeJoinMap(joinStart);

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

			LinkLightingToApi(this, trilist, joinMap);

			LinkLutronQuantumToApi(trilist, joinMap);
		}

		/// <summary>
		/// Links the plugin device specific features to the EISC bridge
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinMap"></param>
		protected void LinkLutronQuantumToApi(BasicTriList trilist, LutronQuantumBridgeJoinMap joinMap)
		{
			Debug.Console(TraceLevel, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(TraceLevel, "Linking to Bridge Type {0}", GetType().Name);

			// link joins to bridge
			trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

			OnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
			CommunicationMonitorFeedback.LinkInputSig(trilist.UShortInput[joinMap.CommunicationMonitorStatus.JoinNumber]);
			if (SocketStatusFeedback != null)
				SocketStatusFeedback.LinkInputSig(trilist.UShortInput[joinMap.SocketStatus.JoinNumber]);

			OnlineFeedback.FireUpdate();
			CommunicationMonitorFeedback.FireUpdate();
			if (SocketStatusFeedback != null)
				SocketStatusFeedback.FireUpdate();

			trilist.OnlineStatusChange += (sender, args) =>
			{
				if (!args.DeviceOnLine) return;

				trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

				OnlineFeedback.FireUpdate();
				CommunicationMonitorFeedback.FireUpdate();
				if (SocketStatusFeedback != null)
					SocketStatusFeedback.FireUpdate();
			};

			trilist.SetStringSigAction(joinMap.IntegrationIdSet.JoinNumber, SetIntegrationId);
			trilist.SetStringSigAction(joinMap.ShadeGroup1IdSet.JoinNumber, SetShadeGroup1Id);
			trilist.SetStringSigAction(joinMap.ShadeGroup2IdSet.JoinNumber, SetShadeGroup2Id);
			trilist.SetStringSigAction(joinMap.Commands.JoinNumber, SendText);

			// raise
			trilist.SetBoolSigAction(joinMap.Raise.JoinNumber, b =>
			{
				if (b)
					MasterRaise();
				else
					MasterRaiseLowerStop();
			});

			// lower
			trilist.SetBoolSigAction(joinMap.Lower.JoinNumber, b =>
			{
				if (b)
					MasterLower();
				else
					MasterRaiseLowerStop();
			});

			// shade group 1 raise, lower
			trilist.SetSigTrueAction(joinMap.ShadeGroup1Raise.JoinNumber, () => ShadeGroupRaise(ShadeGroup1Id));
			trilist.SetSigTrueAction(joinMap.ShadeGroup1Lower.JoinNumber, () => ShadeGroupLower(ShadeGroup1Id));

			// shade group 2 raise, lower
			trilist.SetSigTrueAction(joinMap.ShadeGroup2Raise.JoinNumber, () => ShadeGroupRaise(ShadeGroup2Id));
			trilist.SetSigTrueAction(joinMap.ShadeGroup2Lower.JoinNumber, () => ShadeGroupLower(ShadeGroup2Id));
		}

		#endregion

		// handles login prompts
		private void OnTextReceived(object sender, GenericCommMethodReceiveTextArgs args)
		{
			if (args == null || string.IsNullOrEmpty(args.Text))
			{
				Debug.Console(DebugLevel, this, "OnTextReceived args is null or args.Text is null or empty");
				return;
			}

			try
			{
				if (args.Text.ToLower().Contains("login:"))
				{
					SendText(Username);
				}
				else if (args.Text.ToLower().Contains("password:"))
				{
					SendText(Password);
					_subscribeAfterLogin = new CTimer(x => SubscribeToFeedback(), null, 5000);
				}
				else if (args.Text.ToLower().Contains("access granted"))
				{
					if (_subscribeAfterLogin != null)
					{
						_subscribeAfterLogin.Stop();
					}
					SubscribeToFeedback();
				}

			}
			catch (Exception ex)
			{
				Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Error, "OnTextReceived Exception Message: {0}", ex.Message);
				Debug.Console(VerboseLevel, this, Debug.ErrorLogLevel.Error, "OnTextReceived Exception Stack Trace: {0}", ex.StackTrace);
				if (ex.InnerException != null) Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Error, "OnTextReceived Inner Exception: '{0}'", ex.InnerException);
			}
		}

		// commonly used with ASCII based API's with a defined delimiter				
		private void OnLineRecieved(object sender, GenericCommMethodReceiveTextArgs args)
		{
			if (args == null || string.IsNullOrEmpty(args.Text))
			{
				Debug.Console(DebugLevel, this, "OnLineRecieved args is null or args.Text is null or empty");
				return;
			}

			try
			{
				Debug.Console(DebugLevel, this, "OnLineRecieved args.Text: {0}", args.Text);

				_commsRxQueue.Enqueue(args.Text.ToLower().Contains("~error")
					? new ProcessStringMessage(args.Text, ProcessError)
					: new ProcessStringMessage(args.Text, ProcessResponse));
			}
			catch (Exception ex)
			{
				Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Error, "OnLineRecieved Exception Message: {0}", ex.Message);
				Debug.Console(VerboseLevel, this, Debug.ErrorLogLevel.Error, "OnLineRecieved Exception Stack Trace: {0}", ex.StackTrace);
				if (ex.InnerException != null) Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Error, "OnLineRecieved Inner Exception: '{0}'", ex.InnerException);
			}
		}

		private void ProcessError(string error)
		{
			if (!error.Contains(',')) return;

			var data = error.Split(',');
			if (data == null) return;

			// (pdf pg.13) ~ERROR,{error_number} 
			//	- 1 = parameter count mismatch
			//	- 2 = object does not exist
			//	- 3 = invalid action number
			//	- 4 = parameter data out of range
			//	- 5 = parameter data malformed
			//	- 6 = unsupported command			
			var errNumber = Int32.Parse(data[1]);
			var errMessage = string.Empty;

			switch (errNumber)
			{
				case 1:
					{
						errMessage = "parameter count mismatch";
						break;
					}
				case 2:
					{
						errMessage = "object does not exist";
						break;
					}
				case 3:
					{
						errMessage = "invalid action number";
						break;
					}
				case 4:
					{
						errMessage = "parameter data out of range";
						break;
					}
				case 5:
					{
						errMessage = "parameter data malformed";
						break;
					}
				case 6:
					{
						errMessage = "unsupported command";
						break;
					}
			}

			Debug.Console(DebugLevel, this, "Integration Error[{0}]: {1}", errNumber, errMessage);
		}

		private void ProcessResponse(string response)
		{
			if (string.IsNullOrEmpty(response))
			{
				Debug.Console(VerboseLevel, this, "ProcessResponse: response '{0}' is null or empty", response);
				return;
			}

			try
			{
				if (!response.Contains(',')) return;

				var data = response.Split(',');
				if (data == null) return;

				var command = data[0];
				switch (command.ToLower())
				{
					// (pdf pg.41) ~AREA,{integrationId},{action_number},{parameters}
					case "~area":
						{
							var id = data[1];

							if (id != IntegrationId)
							{
								Debug.Console(VerboseLevel, this, "Response is not for correct Integration ID");
								return;
							}

							var action = Int32.Parse(data[2]);
							if (action == (int)ELutronAction.Scene)
							{
								var scene = data[3];
								CurrentLightingScene = LightingScenes.FirstOrDefault(s => s.ID.Equals(scene));
								OnLightingSceneChange();
							}

							break;
						}
					// (pdf pg.161) ~DEVICE,{integrationId},{action_number},{parameters}
					case "~device":
						{
							var id = data[1];

							ILutronDevice device;
							if (LutronDevices.TryGetValue(id, out device))
							{
								Debug.Console(VerboseLevel, this, "Passing '{1}' to ID-'{0}'", id, data);
								device.ProcessResponse(data);
								return;
							}

							Debug.Console(VerboseLevel, this, "Failed to find device with ID-'{0}'", id);

							break;
						}
				}
			}
			catch (Exception ex)
			{
				Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Error, "ProcessResponse Exception Message: {0}", ex.Message);
				Debug.Console(VerboseLevel, this, Debug.ErrorLogLevel.Error, "ProcessResponse Exception Stack Trace: {0}", ex.StackTrace);
				if (ex.InnerException != null) Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Error, "ProcessResponse Inner Exception: '{0}'", ex.InnerException);
			}
		}

		/// <summary>
		/// Sends text to the device plugin comms
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"SendText", "params":["text"]}
		/// </example>       
		public void SendText(string text)
		{
			if (string.IsNullOrEmpty(text)) return;

			Debug.Console(VerboseLevel, this, "SendText: '{0}'", text);

			var cmd = string.IsNullOrEmpty(CommsDelimiter)
				? string.Format("{0}", text)
				: string.Format("{0}{1}", text, CommsDelimiter);

			_comms.SendText(cmd);
		}

		/// <summary>
		/// Subscribes to feedback
		/// </summary>
		public void SubscribeToFeedback()
		{
			Debug.Console(DebugLevel, this, "Sending monitoring subscriptions");

			SendText("#MONITORING,6,1");
			SendText("#MONITORING,8,1");
			SendText("#MONITORING,5,2");

			foreach (var device in LutronDevices.Values)
			{
				device.DeviceInitialize();
			}
		}

		/// <summary>
		/// Polls the device scene status for the configured zone ID
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"Poll", "params":[]}
		/// </example>
		public void Poll()
		{
			if (string.IsNullOrEmpty(IntegrationId))
			{
				Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Error, "SelectScene: Integration ID ('{0}') is null or empty, verify configuration", IntegrationId);
				return;
			}

			// query scene
			var cmd = string.Format("{0}AREA,{1},{2}", CommsGet, IntegrationId, (int)ELutronAction.Scene);
			SendText(cmd);
		}

		/// <summary>
		/// Scene select
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"SelectScene", "params":[{LightingScene scene}]}
		/// </example>
		public override void SelectScene(LightingScene scene)
		{
			if (scene == null) return;

			if (string.IsNullOrEmpty(IntegrationId))
			{
				Debug.Console(DebugLevel, this, Debug.ErrorLogLevel.Error, "SelectScene: Integration ID ('{0}') is null or empty, verify configuration'", IntegrationId);
				return;
			}

			var cmd = string.Format("{0}AREA,{1},{2},{3}", CommsSet, IntegrationId, (int)ELutronAction.Scene, scene.ID);
			SendText(cmd);
		}

		/// <summary>
		/// Begins raising the lights in the area
		/// </summary>
		public void MasterRaise()
		{
			var cmd = string.Format("{0}AREA,{1},{2}", CommsSet, IntegrationId, (int)ELutronAction.Raise);
			SendText(cmd);
		}

		/// <summary>
		/// Begins lowering the lights in the area
		/// </summary>
		public void MasterLower()
		{
			var cmd = string.Format("{0}AREA,{1},{2}", CommsSet, IntegrationId, (int)ELutronAction.Raise);
			SendText(cmd);
		}

		/// <summary>
		/// Stops the current raise/lower action
		/// </summary>
		public void MasterRaiseLowerStop()
		{
			var cmd = string.Format("{0}AREA,{1},{2}", CommsSet, IntegrationId, (int)ELutronAction.Stop);
			SendText(cmd);
		}

		/// <summary>
		/// Begins raising the shades in the group
		/// </summary>
		public void ShadeGroupRaise(string id)
		{
			var cmd = string.Format("{0}SHADEGRP,{1},{2}", CommsSet, id, (int)ELutronAction.Raise);
			SendText(cmd);
		}

		/// <summary>
		/// Begins lowering the shades in the group
		/// </summary>
		public void ShadeGroupLower(string id)
		{
			var cmd = string.Format("{0}SHADEGRP,{1},{2}", CommsSet, id, (int)ELutronAction.Lower);
			SendText(cmd);
		}

		/// <summary>
		/// Sets the recieved integration id
		/// </summary>
		/// <param name="id"></param>
		public void SetIntegrationId(string id)
		{
			if (String.IsNullOrEmpty(id))
				return;

			var props = JsonConvert.DeserializeObject<LutronQuantumPropertiesConfig>(_deviceConfig.Properties.ToString());
			if (props == null)
			{
				Debug.Console(VerboseLevel, this, "SetIngrationId: failed to deserialize config, unable to save new ID");
				return;
			}

			if (props.IntegrationId.Equals(id))
				return;

			props.IntegrationId = id;
			IntegrationId = id;

			_deviceConfig.Properties = JsonConvert.SerializeObject(props);
			ConfigWriter.UpdateDeviceConfig(_deviceConfig);
		}

		/// <summary>
		/// Sets the recieved shade group 1 id
		/// </summary>
		/// <param name="id"></param>
		public void SetShadeGroup1Id(string id)
		{
			if (String.IsNullOrEmpty(id))
				return;

			var props = JsonConvert.DeserializeObject<LutronQuantumPropertiesConfig>(_deviceConfig.Properties.ToString());
			if (props == null)
			{
				Debug.Console(VerboseLevel, this, "ShadeGroup1IdSet: failed to deserialize config, unable to save new ID");
				return;
			}

			props.ShadeGroup1Id = id;
			ShadeGroup1Id = id;

			_deviceConfig.Properties = JsonConvert.SerializeObject(props);
			ConfigWriter.UpdateDeviceConfig(_deviceConfig);
		}

		/// <summary>
		/// Sets the recieved shade group 2 id
		/// </summary>
		/// <param name="id"></param>
		public void SetShadeGroup2Id(string id)
		{
			if (String.IsNullOrEmpty(id))
				return;

			var props = JsonConvert.DeserializeObject<LutronQuantumPropertiesConfig>(_deviceConfig.Properties.ToString());
			if (props == null)
			{
				Debug.Console(VerboseLevel, this, "ShadeGroup2IdSet: failed to deserialize config, unable to save new ID");
				return;
			}

			props.ShadeGroup2Id = id;
			ShadeGroup2Id = id;

			_deviceConfig.Properties = JsonConvert.SerializeObject(props);
			ConfigWriter.UpdateDeviceConfig(_deviceConfig);
		}

		/// <summary>
		/// Prints the list of scenes to console
		/// </summary>
		/// <example>
		/// devjson:1 {"deviceKey":"{deviceKey}", "methodName":"PrintScenes", "params":[]}
		/// </example>
		public void PrintScenes()
		{
			Debug.Console(TraceLevel, this, new string('*', 80));
			Debug.Console(TraceLevel, this, "Zone ID: {0}", IntegrationId);

			if (LightingScenes == null)
			{
				Debug.Console(TraceLevel, this, "LightingScenes List is null");
				return;
			}

			Debug.Console(TraceLevel, this, "Scene List ({0}-items):", LightingScenes.Count);
			for (var i = 0; i <= LightingScenes.Count; i++)
			{
				Debug.Console(TraceLevel, this, "Scene '{0}': Id-'{1}', Name-'{2}'", i, LightingScenes[i].ID, LightingScenes[i].Name);
			}

			Debug.Console(TraceLevel, this, new string('*', 80));
		}

		/// <summary>
		/// Adds device to dictionary
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="device"></param>
		public void AddDevice(string integrationId, ILutronDevice device)
		{
			LutronDevices.Add(integrationId, device);
		}

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