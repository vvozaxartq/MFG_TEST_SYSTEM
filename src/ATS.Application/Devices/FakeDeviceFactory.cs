using ATS.Core.Devices;

namespace ATS.Application.Devices;

public sealed class FakeDeviceFactory : IDeviceFactory
{
    public IDevice CreateDevice()
    {
        return new FakeDevice();
    }
}
