namespace Imperium.Common
{
    public interface IDevice
    {
        /// <summary>
        /// Read all points and data from the device.
        /// </summary>
        Task Read();

        /// <summary>
        /// Write all points and data to device.
        /// </summary>
        Task Write();
    }
}
