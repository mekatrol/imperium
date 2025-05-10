namespace Imperium.Common.DeviceControllers;

public interface IDeviceControllerFactory
{
    IReadOnlyList<string> GetControllerKeys();

    IDeviceController? AddDeviceController(IServiceProvider services, string controllerKey);
}
