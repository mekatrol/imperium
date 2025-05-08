using Imperium.Common.Models;

namespace Imperium.Common.Services;

public interface IDashboardService
{
    IList<Dashboard> GetAllDashboards();

    Dashboard GetDashboard(string name);
}
