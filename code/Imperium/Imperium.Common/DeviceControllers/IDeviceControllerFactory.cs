namespace Imperium.Common.DeviceControllers;

public interface IDeviceControllerFactory
{
    IDeviceController? AddDeviceController(IServiceProvider services, string controllerKey);
}
