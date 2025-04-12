using System.Text.Json;
using Imperium.Common;

namespace Mekatrol.Devices
{
    public class SimpleOutputBoard : IDevice
    {
        public async Task Initialise(string json)
        {
            dynamic data = JsonSerializer.Deserialize<dynamic>(json)!;
        }

        public async Task Read()
        {
            throw new NotImplementedException();
        }

        public async Task Write()
        {
            throw new NotImplementedException();
        }
    }
}
