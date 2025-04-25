using Imperium.Common.Devices;
using System.Buffers;
using System.Text.RegularExpressions;

namespace Imperium.Common.DeviceControllers;

public interface IMqttDeviceController : IDeviceController
{
    Task ProcessPayload(IDeviceInstance deviceInstance, Match topicMatch, ReadOnlySequence<byte> payload, Points.IPointState pointState);
}
