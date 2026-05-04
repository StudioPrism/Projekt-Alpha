using System;

public static class InventoryItemCatalog
{
    public static readonly InventoryItemDefinition Pistol9Mm = new()
    {
        Id = "weapon.pistol.9mm",
        DisplayName = "9mm Pistol",
        Description = "Compact sidearm. Small enough for a pack pocket, large enough to matter.",
        Category = InventoryCategory.Weapon,
        Footprint = new InventoryFootprint( 2, 3 ),
        CanRotate = true,
        UnitWeight = 1.2f,
        Caliber = "9x19mm",
        Tags = Tags( "weapon", "gun", "pistol", "9mm" )
    };

    public static readonly InventoryItemDefinition HeavyPistol = new()
    {
        Id = "weapon.pistol.heavy",
        DisplayName = "Heavy Pistol",
        Description = "Large-frame pistol with a blockier footprint.",
        Category = InventoryCategory.Weapon,
        Footprint = new InventoryFootprint( 3, 3 ),
        CanRotate = true,
        UnitWeight = 1.8f,
        Tags = Tags( "weapon", "gun", "pistol", "heavy" )
    };

    public static readonly InventoryItemDefinition Rifle556 = new()
    {
        Id = "weapon.rifle.556",
        DisplayName = "5.56 Rifle",
        Description = "Full-length rifle. Efficient, but it eats bag space.",
        Category = InventoryCategory.Weapon,
        Footprint = new InventoryFootprint( 6, 2 ),
        CanRotate = true,
        UnitWeight = 3.5f,
        Caliber = "5.56x45mm",
        Tags = Tags( "weapon", "gun", "rifle", "556" )
    };

    public static readonly InventoryItemDefinition Shotgun12G = new()
    {
        Id = "weapon.shotgun.12g",
        DisplayName = "12 Gauge Shotgun",
        Description = "Long-barrel shotgun. Awkward in a pack, persuasive up close.",
        Category = InventoryCategory.Weapon,
        Footprint = new InventoryFootprint( 6, 2 ),
        CanRotate = true,
        UnitWeight = 3.8f,
        Caliber = "12 Gauge",
        Tags = Tags( "weapon", "gun", "shotgun", "12g" )
    };

    public static readonly InventoryItemDefinition Ammo9MmLoose = new()
    {
        Id = "ammo.9mm.loose",
        DisplayName = "Loose 9mm Rounds",
        Description = "Loose pistol ammunition.",
        Category = InventoryCategory.Ammo,
        Footprint = new InventoryFootprint( 1, 1 ),
        CanRotate = false,
        MaxStack = 50,
        UnitWeight = 0.012f,
        Caliber = "9x19mm",
        Packaging = "Loose",
        Tags = Tags( "ammo", "9mm", "loose" )
    };

    public static readonly InventoryItemDefinition Ammo9MmBox = new()
    {
        Id = "ammo.9mm.box",
        DisplayName = "Boxed 9mm Rounds",
        Description = "Box of pistol ammunition. Larger footprint, better storage density.",
        Category = InventoryCategory.Ammo,
        Footprint = new InventoryFootprint( 2, 1 ),
        CanRotate = true,
        MaxStack = 100,
        UnitWeight = 0.012f,
        Caliber = "9x19mm",
        Packaging = "Box",
        Tags = Tags( "ammo", "9mm", "box" )
    };

    public static readonly InventoryItemDefinition Ammo556Box = new()
    {
        Id = "ammo.556.box",
        DisplayName = "Boxed 5.56 Rounds",
        Description = "Rifle ammunition in a compact box.",
        Category = InventoryCategory.Ammo,
        Footprint = new InventoryFootprint( 2, 2 ),
        CanRotate = true,
        MaxStack = 120,
        UnitWeight = 0.012f,
        Caliber = "5.56x45mm",
        Packaging = "Box",
        Tags = Tags( "ammo", "556", "box" )
    };

    public static readonly InventoryItemDefinition Magazine9Mm = new()
    {
        Id = "magazine.9mm",
        DisplayName = "9mm Magazine",
        Description = "Standard pistol magazine.",
        Category = InventoryCategory.Magazine,
        Footprint = new InventoryFootprint( 1, 2 ),
        CanRotate = true,
        UnitWeight = 0.25f,
        Caliber = "9x19mm",
        Tags = Tags( "magazine", "9mm", "pistol" )
    };

    public static readonly InventoryItemDefinition Medkit = new()
    {
        Id = "medical.medkit",
        DisplayName = "Medkit",
        Description = "Bulky but comprehensive field medical kit.",
        Category = InventoryCategory.Medical,
        Footprint = new InventoryFootprint( 2, 2 ),
        CanRotate = true,
        UnitWeight = 1.0f,
        Tags = Tags( "medical", "healing" )
    };

    public static readonly InventoryItemDefinition Bandage = new()
    {
        Id = "medical.bandage",
        DisplayName = "Bandage",
        Description = "Small emergency dressing.",
        Category = InventoryCategory.Medical,
        Footprint = new InventoryFootprint( 1, 1 ),
        CanRotate = false,
        MaxStack = 5,
        UnitWeight = 0.05f,
        Tags = Tags( "medical", "healing", "stackable" )
    };

    public static readonly InventoryItemDefinition WaterBottle = new()
    {
        Id = "survival.water_bottle",
        DisplayName = "Water Bottle",
        Description = "A tall bottle of clean water.",
        Category = InventoryCategory.Water,
        Footprint = new InventoryFootprint( 1, 2 ),
        CanRotate = true,
        UnitWeight = 0.7f,
        Tags = Tags( "water", "survival" )
    };

    public static readonly InventoryItemDefinition CannedFood = new()
    {
        Id = "survival.canned_food",
        DisplayName = "Canned Food",
        Description = "Shelf-stable calories. Dense, reliable, not glamorous.",
        Category = InventoryCategory.Food,
        Footprint = new InventoryFootprint( 1, 1 ),
        CanRotate = false,
        MaxStack = 3,
        UnitWeight = 0.35f,
        Tags = Tags( "food", "survival", "stackable" )
    };

    public static readonly IReadOnlyList<InventoryItemDefinition> All = new[]
    {
        Pistol9Mm,
        HeavyPistol,
        Rifle556,
        Shotgun12G,
        Ammo9MmLoose,
        Ammo9MmBox,
        Ammo556Box,
        Magazine9Mm,
        Medkit,
        Bandage,
        WaterBottle,
        CannedFood
    };

    public static InventoryItemDefinition Find( string id )
    {
        return All.FirstOrDefault( definition => string.Equals( definition.Id, id, StringComparison.OrdinalIgnoreCase ) );
    }

    private static IReadOnlySet<string> Tags( params string[] tags )
    {
        return new HashSet<string>( tags, StringComparer.OrdinalIgnoreCase );
    }
}
