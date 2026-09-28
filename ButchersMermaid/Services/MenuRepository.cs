using ButchersMermaid.Data;
using ButchersMermaid.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ButchersMermaid.Services;

public interface IMenuRepository
{
    /// <summary>
    /// Loads the full menu, grouped by DisplayGroup ("To Start", "Mains", etc.),
    /// with categories, items, and prices ordered.
    /// </summary>
    Task<List<IGrouping<string, MenuCategory>>> GetGroupedMenuAsync();
}

public class MenuRepository : IMenuRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public MenuRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<IGrouping<string, MenuCategory>>> GetGroupedMenuAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var categories = await db.MenuCategories
            .Include(c => c.Items)
                .ThenInclude(i => i.Prices)
            .AsSplitQuery()
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        // Sort children in memory. EF Core's Include doesn't guarantee order.
        foreach (var category in categories)
        {
            category.Items = category.Items
                .OrderBy(i => i.SortOrder)
                .ToList();

            foreach (var item in category.Items)
            {
                item.Prices = item.Prices
                    .OrderBy(p => p.SortOrder)
                    .ToList();
            }
        }

        return categories
            .GroupBy(c => c.DisplayGroup)
            .ToList();
    }
}