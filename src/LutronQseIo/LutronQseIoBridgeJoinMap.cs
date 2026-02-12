using PepperDash.Essentials.Core;

namespace LutronQuantum
{
	public class LutronQseIoBridgeJoinMap : JoinMapBaseAdvanced
	{
		#region Digital

		/// <summary>
		/// Contact closure
		/// </summary>
		[JoinName("ContactClosure")]
		public JoinDataComplete ContactClosure = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 5
			},
			new JoinMetadata
			{
				Description = "Contact closure feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
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

		#endregion


		public LutronQseIoBridgeJoinMap(uint joinStart)
			: base(joinStart, typeof(LutronQseIoBridgeJoinMap))
		{
		}
	}
}