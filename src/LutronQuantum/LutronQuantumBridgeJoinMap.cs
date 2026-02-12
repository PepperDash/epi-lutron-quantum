using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace LutronQuantum
{
	/// <summary>
	/// Plugin device Bridge Join Map
	/// </summary>
	public class LutronQuantumBridgeJoinMap : GenericLightingJoinMap
	{
		/*
		GenericLightingJoinMap
        
		 [JoinName("IsOnline")] 
		 * JoinNumber = 1
		 * JoinSpan = 1
		 * JoinCapabilities = eJoinCapabilities.ToSIMPL
		 * JoinType = eJoinType.Digital

		[JoinName("SelectScene")]
		 * JoinNumber = 1
		 * JoinSpan = 1
		 * JoinCapabilities = eJoinCapabilities.FromSIMPL
		 * JoinType = eJoinType.Digital

		[JoinName("SelectSceneDirect")]
		 * JoinNumber = 11
		 * JoinSpan = 10
		 * JoinCapabilities = eJoinCapabilities.ToFromSIMPL
		 * JoinType = eJoinType.DigitalSerial

		[JoinName("ButtonVisibility")]
		 * JoinNumber = 41
		 * JoinSpan = 10
		 * JoinCapabilities = eJoinCapabilities.ToSIMPL
		 * JoinType = eJoinType.Digital
	
		[JoinName("IntegrationIdSet")]
		 * JoinNumber = 1
		 * JoinSpan = 1
		 * JoinCapabilities = eJoinCapabilities.FromSIMPL
		 * JoinType = eJoinType.Serial
		*/

		#region Digital

		/// <summary>
		/// Raise lighting level
		/// </summary>
		[JoinName("Raise")]
		public JoinDataComplete Raise = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Raise lighting level",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Lower lighting level
		/// </summary>
		[JoinName("Lower")]
		public JoinDataComplete Lower = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 3,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Raise lighting level",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Shade Group 1 raise
		/// </summary>
		[JoinName("ShadeGroup1Raise")]
		public JoinDataComplete ShadeGroup1Raise = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 61,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Raise Shade Group 1",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Shade Group 1 lower
		/// </summary>
		[JoinName("ShadeGroup1Lower")]
		public JoinDataComplete ShadeGroup1Lower = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 62,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Lower Shade Group 1",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Shade Group 2 raise
		/// </summary>
		[JoinName("ShadeGroup2Raise")]
		public JoinDataComplete ShadeGroup2Raise = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 63,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Raise Shade Group 2",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Shade Group 2 lower
		/// </summary>
		[JoinName("ShadeGroup2Lower")]
		public JoinDataComplete ShadeGroup2Lower = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 64,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Lower Shade Group 2",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		#endregion


		#region Analog

		/// <summary>
		/// Plugin device communication monitor status
		/// </summary>
		[JoinName("CommunicationMonitorStatus")]
		public JoinDataComplete CommunicationMonitorStatus = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Device communication monitor status feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Plugin device communication socket status
		/// </summary>
		[JoinName("SocketStatus")]
		public JoinDataComplete SocketStatus = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Device socket status feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		#endregion


		#region Serial

		/// <summary>
		/// Plugin device name
		/// </summary>
		[JoinName("DeviceName")]
		public JoinDataComplete DeviceName = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Device Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Set shade group 1 ID
		/// </summary>
		[JoinName("shadeGroup1IdSet")]
		public JoinDataComplete ShadeGroup1IdSet = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets the Shade Group 1 ID",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Set shade group 2 ID
		/// </summary>
		[JoinName("shadeGroup2IdSet")]
		public JoinDataComplete ShadeGroup2IdSet = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 3,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Sets the Shade Group 2 ID",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Command Passthru
		/// </summary>
		[JoinName("commands")]
		public JoinDataComplete Commands = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 4,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Command Passthru",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion

		/// <summary>
		/// Plugin device BridgeJoinMap constructor
		/// </summary>
		/// <param name="joinStart">This will be the join it starts on the EISC bridge</param>
		public LutronQuantumBridgeJoinMap(uint joinStart) 
			: base(joinStart, typeof(LutronQuantumBridgeJoinMap))
		{
		}
	}
}