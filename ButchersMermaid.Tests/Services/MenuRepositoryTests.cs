using ButchersMermaid.Data;
using ButchersMermaid.Data.Entities;
using ButchersMermaid.Services;

namespace ButchersMermaid.Tests.Services;

public class MenuRepositoryTests
{
    [Fact]
    public async Task GetGroupedMenuAsync_ReturnsEmpty_WhenNoCategoriesExist()
    {
        using var factory = new TestDbContextFactory();
        var repo = new MenuRepository(factory);

        var groups = await repo.GetGroupedMenuAsync();

        Assert.Empty(groups);
    }

    [Fact]
    public async Task GetGroupedMenuAsync_GroupsCategoriesByDisplayGroup()
    {
        using var factory = new TestDbContextFactory();
        await SeedAsync(factory, db =>
        {
            db.MenuCategories.AddRange(
                new MenuCategory { Name = "Tapas", DisplayGroup = "To Start", SortOrder = 1 },
                new MenuCategory { Name = "Tartare", DisplayGroup = "To Start", SortOrder = 2 },
                new MenuCategory { Name = "Steaks", DisplayGroup = "Mains", SortOrder = 1 });
        });

        var repo = new MenuRepository(factory);
        var groups = await repo.GetGroupedMenuAsync();

        Assert.Equal(2, groups.Count);
        Assert.Equal("To Start", groups[0].Key);
        Assert.Equal(2, groups[0].Count());
        Assert.Equal("Mains", groups[1].Key);
        Assert.Single(groups[1]);
    }

    [Fact]
    public async Task GetGroupedMenuAsync_OrdersCategoriesBySortOrder()
    {
        using var factory = new TestDbContextFactory();
        await SeedAsync(factory, db =>
        {
            db.MenuCategories.AddRange(
                new MenuCategory { Name = "Third", DisplayGroup = "A", SortOrder = 3 },
                new MenuCategory { Name = "First", DisplayGroup = "A", SortOrder = 1 },
                new MenuCategory { Name = "Second", DisplayGroup = "A", SortOrder = 2 });
        });

        var repo = new MenuRepository(factory);
        var groups = await repo.GetGroupedMenuAsync();

        var categories = groups.Single().ToList();
        Assert.Equal("First", categories[0].Name);
        Assert.Equal("Second", categories[1].Name);
        Assert.Equal("Third", categories[2].Name);
    }

    [Fact]
    public async Task GetGroupedMenuAsync_OrdersItemsWithinCategoryBySortOrder()
    {
        using var factory = new TestDbContextFactory();
        await SeedAsync(factory, db =>
        {
            var category = new MenuCategory
            {
                Name = "Steaks",
                DisplayGroup = "Mains",
                SortOrder = 1,
                Items = new List<MenuItem>
                {
                    new() { Name = "Third item", SortOrder = 3, BasePrice = 30m },
                    new() { Name = "First item", SortOrder = 1, BasePrice = 10m },
                    new() { Name = "Second item", SortOrder = 2, BasePrice = 20m },
                },
            };
            db.MenuCategories.Add(category);
        });

        var repo = new MenuRepository(factory);
        var groups = await repo.GetGroupedMenuAsync();

        var items = groups.Single().Single().Items.ToList();
        Assert.Equal("First item", items[0].Name);
        Assert.Equal("Second item", items[1].Name);
        Assert.Equal("Third item", items[2].Name);
    }

    [Fact]
    public async Task GetGroupedMenuAsync_OrdersPricesWithinItemBySortOrder()
    {
        using var factory = new TestDbContextFactory();
        await SeedAsync(factory, db =>
        {
            var item = new MenuItem
            {
                Name = "Rump",
                SortOrder = 1,
                Prices = new List<MenuPrice>
                {
                    new() { PortionLabel = "500g", Amount = 355m, SortOrder = 3 },
                    new() { PortionLabel = "200g", Amount = 230m, SortOrder = 1 },
                    new() { PortionLabel = "300g", Amount = 285m, SortOrder = 2 },
                },
            };
            var category = new MenuCategory
            {
                Name = "Steaks",
                DisplayGroup = "Mains",
                SortOrder = 1,
                Items = new List<MenuItem> { item },
            };
            db.MenuCategories.Add(category);
        });

        var repo = new MenuRepository(factory);
        var groups = await repo.GetGroupedMenuAsync();

        var prices = groups.Single().Single().Items.Single().Prices.ToList();
        Assert.Equal("200g", prices[0].PortionLabel);
        Assert.Equal("300g", prices[1].PortionLabel);
        Assert.Equal("500g", prices[2].PortionLabel);
    }

    /// <summary>Helper: seeds the test DB using a fresh context.</summary>
    private static async Task SeedAsync(
        TestDbContextFactory factory,
        Action<AppDbContext> seed)
    {
        await using var db = await factory.CreateDbContextAsync();
        seed(db);
        await db.SaveChangesAsync();
    }
}