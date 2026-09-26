using ButchersMermaid.Data.Entities;

namespace ButchersMermaid.Data;

/// <summary>
/// Menu seed data. Mirrors menu.md at repo root.
/// Update both files together when the menu changes.
/// </summary>
public static class SeedData
{
    public static IEnumerable<MenuCategory> GetMenu()
    {
        return new List<MenuCategory>
        {
            // -------- To Start --------
            Tapas(),
            Tartare(),

            // -------- Greens --------
            Salads(),

            // -------- Mains --------
            Pasta(),
            Pita(),
            Fish(),
            Burgers(),
            Favourites(),
            Steaks(),

            // -------- Sides --------
            Sides(),

            // -------- Kids --------
            Kids(),
        };
    }

    // ---------------------------------------------------------------
    // Category builders
    // ---------------------------------------------------------------

    private static MenuCategory Tapas() => new()
    {
        Name = "Tapas",
        DisplayGroup = "To Start",
        SortOrder = 1,
        Items = new List<MenuItem>
        {
            Simple("Homemade Bitterballen (5pc)", "Pulled beef covered in panko crumbs", 95, 1),
            Simple("Cheese Croquettes (4pc)", "Homemade croquettes with boerenkaas", 95, 2),
            Simple("Edamame Beans", "Chilli & salt flakes", 105, 3),
            Simple("Marrow Bone", "Garlic butter, served with sourdough", 120, 4),
            Simple("Pork Belly Skewers (3pc)", "Sticky honey & soy dressing", 120, 5),
            Simple("Angry Chicken Wings (6pc)", "FreeRange Chicken — homemade Prego sauce", 120, 6),
            Simple("BBQ Chicken Wings (6pc)", "FreeRange Chicken with homemade BBQ Sauce", 120, 7),
            Simple("Half & Half Chicken Wings", "3 x Angry Wings & 3 x BBQ Wings", 120, 8),
            Simple("Chilli Poppers (3pc)", "Cheese filling wrapped with homemade bacon", 120, 9),
            Simple("Scampiespekki", "3 prawns wrapped in homemade bacon", 125, 10),
            Simple("Lamb Koftas", "4 koftas with wasabi mayo", 125, 11),
            Simple("Grilled Sardines (3pc)", "Served with sourdough", 135, 12),
            WithNote("Cheesy Steak Nachos", "Doritos, peppers, guac & cream cheese", 190, "Change to FreeRange Chicken or Mince — R180", 13),
            Simple("Tataki of Wagyu", "Serve with ponzu sauce, spring onions, fresh ginger", 230, 14),
            Simple("Prosciutto (100g from bone)", "Drizzled hot smoked chilli oil, Parmesan shavings with sourdough", 250, 15),
        },
    };

    private static MenuCategory Tartare() => new()
    {
        Name = "Tartare",
        DisplayGroup = "To Start",
        SortOrder = 2,
        Items = new List<MenuItem>
        {
            Simple("Marrow Tartare", "2 Marrow Bones with layer Beef Tartare", 180, 1),
            Variants("Beef Tartare", "Premixed", 2,
                ("150g", 165m, 1),
                ("300g", 300m, 2)),
            Variants("Smoked Salmon & Trout", "Served with layer avo", 3,
                ("150g", 180m, 1),
                ("300g", 330m, 2)),
            Variants("Surf & Turf", "Beef & Fresh Tuna — with hint of truffles", 4,
                ("150g", 180m, 1),
                ("300g", 330m, 2)),
        },
    };

    private static MenuCategory Salads() => new()
    {
        Name = "Salads",
        DisplayGroup = "Greens",
        SortOrder = 1,
        Items = new List<MenuItem>
        {
            Simple("Salade aux Lardon", "Diced tomato, onions, cucumber and homemade bacon, salad leaves & poached egg", 150, 1),
            Simple("Tuna Salad", "Tuna, olives, peppers & greens", 180, 2),
            Simple("Chicken Salad", "FreeRange chicken, greens, bacon, grated Grana Padano, croutons", 180, 3),
            Simple("Asian Beef Salad", "Sirloin Steak Strips, lettuce, peppers, spring onions, ginger, avo & sesame seeds", 195, 4),
        },
    };

    private static MenuCategory Pasta() => new()
    {
        Name = "Pasta",
        DisplayGroup = "Mains",
        SortOrder = 1,
        Items = new List<MenuItem>
        {
            Simple("Pasta Bolognese", "Homemade Bolognese with Beef Mince. Spaghetti — Gluten free +R20", 170, 1),
            Simple("Pasta Carbonara", "With homemade Guanciale OR Pancetta (no cream). Spaghetti — Gluten free +R20", 180, 2),
            Simple("Salmon Pasta", "Creamy mushroom & garlic sauce. Spaghetti — Gluten free +R20", 200, 3),
        },
    };

    private static MenuCategory Pita() => new()
    {
        Name = "Pita",
        DisplayGroup = "Mains",
        SortOrder = 2,
        Items = new List<MenuItem>
        {
            Simple("Chicken Pita", "Crunchy cabbage mix, garlicky mayo sauce", 165, 1),
            Simple("Lamb Pita", "Crunchy cabbage mix, garlicky mayo sauce", 175, 2),
        },
    };

    private static MenuCategory Fish() => new()
    {
        Name = "Fish",
        DisplayGroup = "Mains",
        SortOrder = 3,
        Items = new List<MenuItem>
        {
            Simple("Grilled Tikka Prawns", "With Thai Curry & Coconut Jasmine Rice", 300, 1),
            Simple("Grilled Kingklip", "Tomato basil sauce on bed of mash", 300, 2),
            MarketPrice("Canadian Sole Meunière", "Rolled in flour & pan fried with capers & farm butter, served with fries & small salad", 3),
        },
    };

    private static MenuCategory Burgers() => new()
    {
        Name = "Burgers",
        DisplayGroup = "Mains",
        SortOrder = 4,
        Items = new List<MenuItem>
        {
            Simple("Smash Burger", "Double smash, cheese, lettuce & gherkins", 140, 1),
            Simple("Classic Beef OR Lamb", "Patty, lettuce, tomato, onion, gherkins", 140, 2),
            Simple("Chicken Fillet Burger", "FreeRange Chicken (grilled), lettuce, tomato, onion, gherkins", 175, 3),
            Simple("Signature Burger", "Double Beef Patty, bacon & cheese, lettuce, tomato, onion, gherkins", 190, 4),
            Simple("Wagyu Burger", "Caramelized onions, aioli, cheese, with lettuce, tomato, onions & gherkins", 210, 5),
            Simple("Vegetarian Burger", "Urban Vegan Burgers — Lentils & Mushrooms (contains gluten)", 170, 6),
            Simple("Shroomy Burger", "Patty, sautéed garlicky mushrooms, caramelized onions, mushroom cheesy sauce", 205, 7),
            Simple("Quattro Formaggi | 4 Cheese", "Patty, Gorgonzola, Parmigiana, Mozzarella & Cheddar to sweeten it up", 205, 8),
            Simple("Captain Crunch", "Nachos… but as a burger! Patty with spicy mix for that extra kick", 205, 9),
            Simple("Surf & Turf Burger", "Chorizo infused flavoured beef burger patty topped with mozzarella & garlicky prawns", 220, 10),
        },
    };

    private static MenuCategory Favourites() => new()
    {
        Name = "Favourites",
        DisplayGroup = "Mains",
        SortOrder = 5,
        Items = new List<MenuItem>
        {
            Simple("FreeRange Chicken Fillet", "Grilled chicken with sautéed mushrooms, served with fritz OR mash", 200, 1),
            Simple("Pork Spare Ribs (500g)", "Homemade basting served with fritz", 260, 2),
            Simple("Lamb Shank", "Moroccan flavours with cous cous", 275, 3),
            Simple("Garlicky Lamb Cutlets (3 cutlets)", "Served with seasonal roast veggies, gravy mash OR fries", 320, 4),
            Simple("Grilled Lamb Rump (250g)", "Served with carrots, Brussels sprouts, baby potatoes, Rosa tomatoes and a rich garlic lamb sauce", 320, 5),
            Simple("Kudu Fillet (240g)", "Served with Carrot Mash, mushrooms & a Bordelaise Sauce", 345, 6),
        },
    };

    private static MenuCategory Steaks() => new()
    {
        Name = "Steaks",
        DisplayGroup = "Mains",
        SortOrder = 6,
        Items = new List<MenuItem>
        {
            Variants("Rump (A-Grade Beef)", null, 1,
                ("200g", 230m, 1), ("300g", 285m, 2), ("500g", 355m, 3)),
            Variants("Sirloin (A-Grade Beef)", null, 2,
                ("200g", 230m, 1), ("300g", 285m, 2), ("500g", 355m, 3)),
            Variants("Fillet (A-Grade Beef)", null, 3,
                ("200g", 280m, 1), ("300g", 350m, 2)),
            Simple("Ribeye (A-Grade Beef) — 300g", null, 350, 4),
            Simple("Sirloin (DryAged on bone)", "A-Grade Beef", 360, 5),
            Simple("T-Bone (DryAged on bone)", "A-Grade Beef", 460, 6),
            Variants("Rump (FreeRange Beef)", null, 7,
                ("200g", 265m, 1), ("300g", 320m, 2), ("500g", 500m, 3)),
            Variants("Sirloin (FreeRange Beef)", null, 8,
                ("200g", 265m, 1), ("300g", 320m, 2)),
            Variants("Fillet (FreeRange Beef)", null, 9,
                ("200g", 370m, 1), ("300g", 465m, 2)),
            Simple("Ribeye (FreeRange Beef) — 300g", null, 465, 10),
            Simple("Sirloin (on bone)", "FreeRange Beef", 525, 11),
            Simple("Wagyu Ribeye (225g)", "FreeRange Beef", 725, 12),
            Simple("Grilled Marrow Bone", "Steak add-on with garlic butter", 70, 13),
            Simple("Grilled Queen Prawns (2pc)", "Steak add-on, deshelled with garlic butter", 70, 14),
        },
    };

    private static MenuCategory Sides() => new()
    {
        Name = "Sides",
        DisplayGroup = "On the Side",
        SortOrder = 1,
        Items = new List<MenuItem>
        {
            Simple("Café de Paris Butter", "Mixed herbs with farm butter", 50, 1),
            Simple("Marrow Bone Butter", "Steak sauce", 50, 2),
            Simple("Green Peppercorn Sauce", "Steak sauce", 55, 3),
            Simple("Mushroom Sauce", "Steak sauce", 55, 4),
            Simple("Blue Cheese Sauce", "Steak sauce", 55, 5),
            Simple("Bordelaise Sauce", "Red wine sauce with shallots and herbs", 55, 6),
            Simple("Béarnaise", "Creamy, buttery sauce with tarragon and herbs", 55, 7),
            Simple("Chimichurri", "Tangy sauce with parsley, garlic, vinegar, and olive oil", 55, 8),
            Simple("Mashed Potato", "Creamy mashed potatoes", 50, 9),
            Simple("Baked Potato", "Filled with Crème Fraîche", 50, 10),
            Simple("Pommes Frites", "Potato variation", 50, 11),
            Simple("Potato Croquettes (3pc)", "Potato variation", 50, 12),
            Simple("Mixed Salad", "Baby salad, tomato, cucumber, carrots, with homemade salad dressing", 50, 13),
            Simple("Wok 'n' Roll Cabbage", "Crazy cabbage strips tossed with our homemade smokey bacon & soy", 65, 14),
            Simple("Soy Shrooms", "Sautéed mushrooms, glazed in soy with dry herb medley", 65, 15),
            Simple("Cauli & Chilli", "Cauliflower tossed in garlic butter with a hint of chilli", 65, 16),
            Simple("Roasted Mixed Veggies", "Seasonal fresh vegetables", 65, 17),
        },
    };

    private static MenuCategory Kids() => new()
    {
        Name = "Kids (Under 12)",
        DisplayGroup = "For the Young Ones",
        SortOrder = 1,
        Items = new List<MenuItem>
        {
            Simple("Chicken Strips", "FreeRange Chicken served with fries", 90, 1),
            Simple("Classic Beef / Lamb Burger", "100g beef patty served with fries", 100, 2),
            Simple("Pasta Bolognese", "Homemade Bolognese with Beef Mince. Spaghetti — Gluten free +R15", 110, 3),
        },
    };

    // ---------------------------------------------------------------
    // Helper factories — named so calls read clearly
    // ---------------------------------------------------------------

    /// <summary>Item with a single price.</summary>
    private static MenuItem Simple(string name, string? description, decimal price, int sort) => new()
    {
        Name = name,
        Description = description,
        BasePrice = price,
        SortOrder = sort,
    };

    /// <summary>Item with a single price plus a free-text note shown after the price.</summary>
    private static MenuItem WithNote(string name, string? description, decimal price, string note, int sort) => new()
    {
        Name = name,
        Description = description,
        BasePrice = price,
        PriceNote = note,
        SortOrder = sort,
    };

    /// <summary>Item with no numeric price — shows "Market price" instead.</summary>
    private static MenuItem MarketPrice(string name, string? description, int sort) => new()
    {
        Name = name,
        Description = description,
        BasePrice = null,
        PriceNote = "Market price",
        SortOrder = sort,
    };

    /// <summary>Item with multiple portion/price variants.</summary>
    private static MenuItem Variants(
        string name,
        string? description,
        int sort,
        params (string? Portion, decimal Amount, int Sort)[] prices) => new()
    {
        Name = name,
        Description = description,
        BasePrice = null,
        SortOrder = sort,
        Prices = prices.Select(p => new MenuPrice
        {
            PortionLabel = p.Portion,
            Amount = p.Amount,
            SortOrder = p.Sort,
        }).ToList(),
    };
}