namespace Mekatrol.Devices;

internal class SunriseSunsetModel
{
    public string Status { get; set; } = string.Empty;

    public string Tzid { get; set; } = string.Empty;

    public SunriseSunsetResultsModel Results { get; set; } = new();
}
