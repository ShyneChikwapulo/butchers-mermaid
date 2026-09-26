
namespace ButchersMermaid.Data.Entities;

public class MenuCategory
{
    public int Id { get; set; }

    /// <summary>Category name as displayed on the menu, e.g. "Tapas".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Higher-level grouping for page layout, e.g. "To Start", "Mains".
    /// Owner-facing menu keeps original categories; the site groups them.
    /// </summary>
    public string DisplayGroup { get; set; } = string.Empty;

    /// <summary>Ordering within DisplayGroup. Lower = earlier.</summary>
    public int SortOrder { get; set; }

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}