
namespace PepperDash.Essentials.Plugins
{
	public interface ILutronDevice
	{
		void DeviceInitialize();
		void ProcessResponse(string[] message);
	}
}