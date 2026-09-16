using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace LutronQuantum
{
	/// <summary>
	/// Bridge join map for the multi-area comms device — the object that owns the connection but
	/// no area of its own.
	/// </summary>
	/// <remarks>
	/// A strict subset of <see cref="LutronQuantumBridgeJoinMap"/>, keeping the joins that do
	/// something on a comms-only device at the numbers they already had, and dropping the ones that
	/// cannot work here.
	///
	/// Dropped, because this device has no area ID and no shade group IDs — every one of these hits
	/// an empty-ID guard and returns, so exposing them invites SIMPL to be wired to a dead join:
	///   Digital 1 SelectScene, 2 Raise, 3 Lower, 11-20 SelectSceneDirect, 41-50 ButtonVisibility,
	///   61-64 shade group raise/lower; Serial 1 IntegrationIdSet, 2 and 3 shade group ID set.
	///
	/// Lighting, shades and buttons are reached through the per-room child devices instead.
	/// </remarks>
	public class LutronQuantumCommsJoinMap : JoinMapBaseAdvanced
	{
		#region Digital

		/// <summary>
		/// Online status of the connection
		/// </summary>
		[JoinName("IsOnline")]
		public JoinDataComplete IsOnline = new JoinDataComplete(
			new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Is online",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		#endregion

		#region Analog

		/// <summary>
		/// Communication monitor status
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
		/// Socket status
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
		/// Device name
		/// </summary>
		[JoinName("DeviceName")]
		public JoinDataComplete DeviceName = new JoinDataComplete(
			new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Device Name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Address the device is connected to
		/// </summary>
		/// <remarks>
		/// Serial 2 to match the Lutron LEAP plugin, which reports its address on the same join.
		/// Empty when the device is on RS232, since there is no address to report — the description
		/// says so rather than leaving an integrator to read a blank join as a fault.
		/// </remarks>
		[JoinName("DeviceIpAddress")]
		public JoinDataComplete DeviceIpAddress = new JoinDataComplete(
			new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Device IP address (TCP connections only; empty on RS232)",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Raw command passthrough
		/// </summary>
		/// <remarks>
		/// Kept at serial 4, where the single-area map has always put it, so anything already wired
		/// to that join keeps working.
		/// </remarks>
		[JoinName("commands")]
		public JoinDataComplete Commands = new JoinDataComplete(
			new JoinData { JoinNumber = 4, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Command Passthru",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="joinStart">Join this map starts on within the EISC bridge</param>
		public LutronQuantumCommsJoinMap(uint joinStart)
			: base(joinStart, typeof(LutronQuantumCommsJoinMap))
		{
		}
	}
}
