
namespace ButchersMermaid.Data.Entities;

public class MenuItem
{
    public int Id { get; set; }

    public int MenuCategoryId { get; set; }
    public MenuCategory Category { get; set; } = null!;

    /// <summary>Item name, e.g. "Wagyu Ribeye".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description, e.g. "FreeRange Beef". Null if none.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Single price (Rands) for items with one price. Null for items with
    /// multiple price variants (steaks, tartare) — those use <see cref="Prices"/>.
    /// </summary>
    public decimal? BasePrice { get; set; }

    /// <summary>Portion/price variants for items with multiple options.</summary>
    public ICollection<MenuPrice> Prices { get; set; } = new List<MenuPrice>();

    /// <summary>Ordering within the category. Lower = earlier.</summary>
    public int SortOrder { get; set; }

    /// <summary>Free-text notes displayed after the price, e.g. "Market price".</summary>
    public string? PriceNote { get; set; }
}