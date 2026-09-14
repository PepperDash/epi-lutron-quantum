using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace LutronQuantum
{
	/// <summary>
	/// Bridge join map for one room's shade groups.
	/// </summary>
	/// <remarks>
	/// Shade groups are fire and forget — the Lutron system reports no position back, so there is no
	/// feedback here beyond online status and the group names. The "All" joins drive every
	/// configured group in one press.
	/// </remarks>
	public class LutronQuantumShadeAreaJoinMap : JoinMapBaseAdvanced
	{
		#region Digital

		/// <summary>
		/// Online status of the shared connection
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

		/// <summary>
		/// Raise every configured shade group
		/// </summary>
		[JoinName("AllRaise")]
		public JoinDataComplete AllRaise = new JoinDataComplete(
			new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Raise all shade groups in this room",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Lower every configured shade group
		/// </summary>
		[JoinName("AllLower")]
		public JoinDataComplete AllLower = new JoinDataComplete(
			new JoinData { JoinNumber = 3, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Lower all shade groups in this room",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Stop every configured shade group
		/// </summary>
		[JoinName("AllStop")]
		public JoinDataComplete AllStop = new JoinDataComplete(
			new JoinData { JoinNumber = 4, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Stop all shade groups in this room",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Raise one shade group
		/// </summary>
		[JoinName("ShadeGroupRaise")]
		public JoinDataComplete ShadeGroupRaise = new JoinDataComplete(
			new JoinData { JoinNumber = 11, JoinSpan = 10 },
			new JoinMetadata
			{
				Description = "Raise shade group (1-10)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Lower one shade group
		/// </summary>
		[JoinName("ShadeGroupLower")]
		public JoinDataComplete ShadeGroupLower = new JoinDataComplete(
			new JoinData { JoinNumber = 21, JoinSpan = 10 },
			new JoinMetadata
			{
				Description = "Lower shade group (1-10)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Stop one shade group
		/// </summary>
		[JoinName("ShadeGroupStop")]
		public JoinDataComplete ShadeGroupStop = new JoinDataComplete(
			new JoinData { JoinNumber = 31, JoinSpan = 10 },
			new JoinMetadata
			{
				Description = "Stop shade group (1-10)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
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
				Description = "Device name",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Shade group names
		/// </summary>
		[JoinName("ShadeGroupName")]
		public JoinDataComplete ShadeGroupName = new JoinDataComplete(
			new JoinData { JoinNumber = 11, JoinSpan = 10 },
			new JoinMetadata
			{
				Description = "Shade group name (1-10)",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="joinStart">Join this map starts on within the EISC bridge</param>
		public LutronQuantumShadeAreaJoinMap(uint joinStart)
			: base(joinStart, typeof(LutronQuantumShadeAreaJoinMap))
		{
		}
	}
}
