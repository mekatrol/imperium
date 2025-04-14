namespace Mekatrol.Devices;

internal class SunriseSunsetResultsModel
{
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }
    public DateTime SolarNoon { get; set; }
    public int DayLength { get; set; } = 0;
    public DateTime CivilTwilightBegin { get; set; }
    public DateTime CivilTwilightEnd { get; set; }
    public DateTime NauticalTwilightBegin { get; set; }
    public DateTime NauticalTwilightEnd { get; set; }
    public DateTime AstronomicalTwilightBegin { get; set; }
    public DateTime AstronomicalTwilightEnd { get; set; }
}
