namespace Imperium.Common.Models;

public class DashboardItem
{
    public string ComponentName { get; set; } = string.Empty;

    public int Column { get; set; }

    public int ColumnSpan { get; set; }

    public int Row { get; set; }

    public int RowSpan { get; set; }

    public string? CssClass { get; set; }

    public object? Props { get; set; }
}

