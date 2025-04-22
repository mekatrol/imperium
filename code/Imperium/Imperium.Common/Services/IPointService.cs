using Imperium.Common.Points;

namespace Imperium.Common.Services;

public interface IPointService
{
    Task<Point> UpdatePoint(PointUpdateValueModel pointUpdate);
}
