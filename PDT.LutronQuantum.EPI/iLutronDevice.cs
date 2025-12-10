
namespace LutronQuantum
{
	public interface ILutronDevice
	{
		void DeviceInitialize();
		void ProcessResponse(string[] message);
	}
}