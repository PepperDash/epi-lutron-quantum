using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace LutronQuantum
{
	/// <summary>
	/// Bridge join map for a single lighting area of a multi-area system.
	/// </summary>
	/// <remarks>
	/// Deliberately keeps the scene layout inherited from <see cref="GenericLightingJoinMap"/> —
	/// scene select at 11 and visibility at 41 — so an area bridges the same way a single-area
	/// <c>lutronQuantum</c> device always has. Shades and buttons are separate devices with their
	/// own maps, so nothing beyond lighting appears here.
	///
	/// Inherited from GenericLightingJoinMap:
	///   Digital  IsOnline            1
	///   Digital  SelectScene         1
	///   Digital  SelectSceneDirect  11 (span 10)
	///   Digital  ButtonVisibility   41 (span 10)
	///   Serial   IntegrationIdSet    1
	/// </remarks>
	public class LutronQuantumAreaJoinMap : GenericLightingJoinMap
	{
		#region Digital

		/// <summary>
		/// Raise the area while held, stop on release
		/// </summary>
		[JoinName("Raise")]
		public JoinDataComplete Raise = new JoinDataComplete(
			new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Raise lighting level while held, stop on release",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Lower the area while held, stop on release
		/// </summary>
		[JoinName("Lower")]
		public JoinDataComplete Lower = new JoinDataComplete(
			new JoinData { JoinNumber = 3, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Lower lighting level while held, stop on release",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		#endregion

		#region Analog

		/// <summary>
		/// Communication monitor status of the shared connection
		/// </summary>
		[JoinName("CommunicationMonitorStatus")]
		public JoinDataComplete CommunicationMonitorStatus = new JoinDataComplete(
			new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Device communication monitor status feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Socket status of the shared connection
		/// </summary>
		[JoinName("SocketStatus")]
		public JoinDataComplete SocketStatus = new JoinDataComplete(
			new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Device socket status feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		#endregion

		#region Serial

		/// <summary>
		/// Area name
		/// </summary>
		[JoinName("DeviceName")]
		public JoinDataComplete DeviceName = new JoinDataComplete(
			new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Area name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="joinStart">Join this map starts on within the EISC bridge</param>
		public LutronQuantumAreaJoinMap(uint joinStart)
			: base(joinStart, typeof(LutronQuantumAreaJoinMap))
		{
		}
	}
}
