
namespace ButchersMermaid.Data.Entities;

public class MenuPrice
{
    public int Id { get; set; }

    public int MenuItemId { get; set; }
    public MenuItem Item { get; set; } = null!;

    /// <summary>Portion label, e.g. "200g", "300g", "500g". Null for price-only variants.</summary>
    public string? PortionLabel { get; set; }

    /// <summary>Price in Rands.</summary>
    public decimal Amount { get; set; }

    /// <summary>Ordering within the item. Lower = earlier.</summary>
    public int SortOrder { get; set; }
}