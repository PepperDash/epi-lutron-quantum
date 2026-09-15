using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace LutronQuantum
{
	/// <summary>
	/// Bridge join map for a single lighting area of a multi-area system.
	/// </summary>
	/// <remarks>
	/// Declares every join rather than inheriting <c>GenericLightingJoinMap</c>. That map was
	/// written when a lighting device was the whole controller, so its descriptions all read
	/// "Lighting Controller ..." — on a per-area child only the online join is controller-wide (it
	/// reports the shared NWK connection); everything else belongs to one area. It also declares
	/// SelectScene as Digital while <c>LinkLightingToApi</c> wires it with
	/// <c>SetUShortSigAction</c>, so the generated documentation sends integrators to the wrong
	/// signal type.
	///
	/// Join numbers are unchanged from that map, so this is a re-description rather than a
	/// re-layout: scene select on 11, visibility on 41, raise/lower on 2 and 3.
	/// </remarks>
	public class LutronQuantumAreaJoinMap : JoinMapBaseAdvanced
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
				Description = "Lighting system online (shared connection, not per area)",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Raise the area while held
		/// </summary>
		[JoinName("Raise")]
		public JoinDataComplete Raise = new JoinDataComplete(
			new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Area raise while held, stop on release",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Lower the area while held
		/// </summary>
		[JoinName("Lower")]
		public JoinDataComplete Lower = new JoinDataComplete(
			new JoinData { JoinNumber = 3, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Area lower while held, stop on release",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Recall a scene, scene-active feedback, and the scene name
		/// </summary>
		/// <remarks>
		/// DigitalSerial, so this occupies both halves: the digital recalls the scene and reports
		/// high while it is active, and the serial at the same number carries its name.
		/// </remarks>
		[JoinName("SelectSceneDirect")]
		public JoinDataComplete SelectSceneDirect = new JoinDataComplete(
			new JoinData { JoinNumber = 11, JoinSpan = 10 },
			new JoinMetadata
			{
				Description = "Recall area scene (1-10), high while active; serial carries the scene name",
				JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
				JoinType = eJoinType.DigitalSerial
			});

		/// <summary>
		/// Which scene slots are configured
		/// </summary>
		[JoinName("ButtonVisibility")]
		public JoinDataComplete ButtonVisibility = new JoinDataComplete(
			new JoinData { JoinNumber = 41, JoinSpan = 10 },
			new JoinMetadata
			{
				Description = "Area scene (1-10) is configured - visibility for the matching join 11-20",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
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
				Description = "Communication monitor status feedback (shared connection)",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Recall a scene by number
		/// </summary>
		/// <remarks>
		/// Analog, not digital — the inherited map declared this as a digital while the framework
		/// wired it as an analog.
		///
		/// The value is 1-based with zero reserved, matching how <c>DisplayBase</c> treats its own
		/// analog select. The framework's lighting helper is zero-based instead, which is unsafe on
		/// an EISC: SIMPL can push a zero on an analog join as the bridge comes online, and that
		/// would recall the first scene every time the link re-established. Here zero does nothing,
		/// and a number past the last configured scene is logged and ignored rather than throwing.
		/// </remarks>
		[JoinName("SelectSceneByIndex")]
		public JoinDataComplete SelectSceneByIndex = new JoinDataComplete(
			new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Recall area scene by number (1 = first configured scene; 0 is ignored)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
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
				Description = "Socket status feedback (shared connection)",
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

		/// <summary>
		/// Set the area's integration ID at runtime
		/// </summary>
		[JoinName("AreaIdSet")]
		public JoinDataComplete AreaIdSet = new JoinDataComplete(
			new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Set the area integration ID",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
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
