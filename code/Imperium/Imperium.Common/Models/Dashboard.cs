namespace Imperium.Common.Models;

public partial class Dashboard
{
    public string Name { get; set; } = string.Empty;

    public List<DashboardItem> Items { get; set; } = [];
}
