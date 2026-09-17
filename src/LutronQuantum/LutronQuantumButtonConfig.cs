using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Plugins
{
	/// <summary>
	/// Configuration for one bridgeable button, which drives one or more real keypad buttons.
	/// </summary>
	/// <remarks>
	/// Lutron programs frequently expose shades as separate per-wall buttons with no group object.
	/// Listing several targets here recreates that grouping in the plugin, so a single join can send
	/// "everything up" as consecutive <c>#DEVICE</c> commands.
	/// </remarks>
	public class LutronQuantumButtonConfig
	{
		/// <summary>
		/// Display name, reported on the button's serial feedback join.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// Order the button occupies on the bridge. Buttons are sorted ascending before being
		/// assigned to joins, so this controls which join a button lands on.
		/// </summary>
		[JsonProperty("sortOrder")]
		public int SortOrder { get; set; }

		/// <summary>
		/// How the button behaves when driven. Defaults to
		/// <see cref="EButtonAction.PressAndRelease"/> when omitted.
		/// </summary>
		[JsonProperty("buttonAction")]
		[JsonConverter(typeof(StringEnumConverter))]
		public EButtonAction? ButtonAction { get; set; }

		/// <summary>
		/// How pressed feedback is reported. Defaults to <see cref="EButtonFeedbackMode.Echo"/>.
		/// </summary>
		[JsonProperty("feedbackMode")]
		[JsonConverter(typeof(StringEnumConverter))]
		public EButtonFeedbackMode? FeedbackMode { get; set; }

		/// <summary>
		/// The keypad buttons this button drives, sent in list order.
		/// </summary>
		[JsonProperty("targets")]
		public List<LutronQuantumButtonTargetConfig> Targets { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public LutronQuantumButtonConfig()
		{
			Targets = new List<LutronQuantumButtonTargetConfig>();
		}
	}
}
