using Imperium.Common.Utils;

namespace Imperium.Tests.Unit;

[TestClass]
public sealed class DateTimeUtilsTests
{
    [TestMethod]
    public void SameTime()
    {
        var now = DateTime.Parse("1 July 2025 12:02:23.0000");

        var start = TimeOnly.Parse("12:02:23.0000");
        var end = TimeOnly.Parse("12:02:23.0000");
        Assert.IsFalse(now.WithinTimeRange(start, end));
    }

    [TestMethod]
    public void SameDay()
    {
        var start = TimeOnly.Parse("12:00:00.0000");
        var end = TimeOnly.Parse("23:00:00.0000");
        Assert.IsFalse(DateTime.Parse("1 July 2025 11:59:59.9999").WithinTimeRange(start, end));
        Assert.IsTrue(DateTime.Parse("1 July 2025 12:00:00.0000").WithinTimeRange(start, end));
        Assert.IsTrue(DateTime.Parse("1 July 2025 12:01:00.0000").WithinTimeRange(start, end));
        Assert.IsTrue(DateTime.Parse("1 July 2025 22:59:59.9999").WithinTimeRange(start, end));
        Assert.IsTrue(DateTime.Parse("1 July 2025 23:00:00.0000").WithinTimeRange(start, end));
        Assert.IsFalse(DateTime.Parse("1 July 2025 23:00:00.0001").WithinTimeRange(start, end));
    }

    [TestMethod]
    public void OverNight()
    {
        var start = TimeOnly.Parse("23:00:00.0000");
        var end = TimeOnly.Parse("12:00:00.0000");
        Assert.IsFalse(DateTime.Parse("1 July 2025 22:59:59.9999").WithinTimeRange(start, end));
        Assert.IsTrue(DateTime.Parse("1 July 2025 23:00:00.0000").WithinTimeRange(start, end));
        Assert.IsTrue(DateTime.Parse("1 July 2025 23:00:00.0001").WithinTimeRange(start, end));
        Assert.IsTrue(DateTime.Parse("1 July 2025 11:59:59.9999").WithinTimeRange(start, end));
        Assert.IsTrue(DateTime.Parse("1 July 2025 12:00:00.0000").WithinTimeRange(start, end));
        Assert.IsFalse(DateTime.Parse("1 July 2025 12:00:00.0001").WithinTimeRange(start, end));
    }
}
