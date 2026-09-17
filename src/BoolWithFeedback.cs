using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugins
{
	public class BoolWithFeedback
	{
		public BoolFeedback Feedback;
		private bool _Value;
		public bool Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				Feedback.FireUpdate();
				
			}
		}
		public BoolWithFeedback()
		{
            Feedback = new BoolFeedback("BoolFeedback",() =>
            {
                return Value;
			});
		}
	}
}