namespace Imperium.Common.Utils;

public static class DateTimeUtils
{
    public static bool WithinTimeRange(this DateTime dateTime, TimeOnly start, TimeOnly end)
    {
        var timeOnly = TimeOnly.FromDateTime(dateTime);

        // There is no range
        if (start == end)
        {
            return false;
        }

        // Testing range within the same day
        if (start < end)
        {
            return timeOnly >= start && timeOnly <= end;
        }

        // Testing range that spans overnight
        return timeOnly >= start || timeOnly <= end ;
    }
}
