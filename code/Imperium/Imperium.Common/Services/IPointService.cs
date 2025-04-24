using Imperium.Common.Points;

namespace Imperium.Common.Services;

public interface IPointService
{
    IList<Point> GetAllPoints();

    Task<Point> UpdatePoint(PointUpdateValueModel pointUpdate);
}
