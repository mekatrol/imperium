using Imperium.Common.Models;
using Imperium.Common.Points;
using Microsoft.Extensions.DependencyInjection;

namespace Imperium.Common.Services;
internal class DashboardService(IServiceProvider services) : IDashboardService
{
    public IList<Dashboard> GetAllDashboards()
    {
        var state = services.GetRequiredService<IImperiumState>();
        return state.GetAllDashboards();
    }

    public Dashboard GetDashboard(string name)
    {
        var state = services.GetRequiredService<IImperiumState>();
        return state.GetDashboard(name);
    }
}
