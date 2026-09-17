using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Plugins
{
	/// <summary>
	/// Bridge join map for one room's emulated keypad buttons.
	/// </summary>
	/// <remarks>
	/// One digital per configured button, in sort order. Both edges are used, so a button configured
	/// as PressAndHold stays held for as long as SIMPL holds the join — which is what stops shade
	/// travel on release. The digital also carries pressed feedback and the matching serial carries
	/// the button name.
	/// </remarks>
	public class LutronQuantumButtonGroupJoinMap : JoinMapBaseAdvanced
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
		/// Button press and pressed feedback
		/// </summary>
		[JoinName("ButtonPress")]
		public JoinDataComplete ButtonPress = new JoinDataComplete(
			new JoinData { JoinNumber = 11, JoinSpan = 20 },
			new JoinMetadata
			{
				Description = "Button press and pressed feedback (1-20)",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
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
		/// Button names
		/// </summary>
		[JoinName("ButtonName")]
		public JoinDataComplete ButtonName = new JoinDataComplete(
			new JoinData { JoinNumber = 11, JoinSpan = 20 },
			new JoinMetadata
			{
				Description = "Button name (1-20)",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="joinStart">Join this map starts on within the EISC bridge</param>
		public LutronQuantumButtonGroupJoinMap(uint joinStart)
			: base(joinStart, typeof(LutronQuantumButtonGroupJoinMap))
		{
		}
	}
}
