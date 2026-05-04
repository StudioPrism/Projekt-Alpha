using System;

public sealed class InventoryItemDefinition
{
    public string Id { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Description { get; init; } = "";
    public string IconPath { get; init; } = "";
    public InventoryCategory Category { get; init; }
    public InventoryFootprint Footprint { get; init; } = new InventoryFootprint( 1, 1 );
    public bool CanRotate { get; init; } = true;
    public int MaxStack { get; init; } = 1;
    public float UnitWeight { get; init; } = 0.1f;
    public string Caliber { get; init; } = "";
    public string Packaging { get; init; } = "";
    public IReadOnlySet<string> Tags { get; init; } = new HashSet<string>();

    public bool IsStackable => MaxStack > 1;

    public InventoryFootprint GetFootprint( bool rotated )
    {
        return rotated && CanRotate ? Footprint.Rotated : Footprint;
    }

    public void Validate()
    {
        if ( string.IsNullOrWhiteSpace( Id ) )
            throw new InvalidOperationException( "Inventory item definitions require a stable Id." );

        if ( MaxStack < 1 )
            throw new InvalidOperationException( $"Inventory item '{Id}' has an invalid MaxStack." );

        if ( UnitWeight < 0f )
            throw new InvalidOperationException( $"Inventory item '{Id}' has a negative UnitWeight." );
    }
}
