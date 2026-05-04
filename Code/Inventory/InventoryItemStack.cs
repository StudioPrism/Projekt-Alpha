using System;

public sealed class InventoryItemStack
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public InventoryItemDefinition Definition { get; }
    public int Quantity { get; private set; }
    public bool IsRotated { get; private set; }
    public InventoryGridPosition Position { get; private set; }

    public InventoryItemStack( InventoryItemDefinition definition, int quantity = 1 )
    {
        Definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
        Definition.Validate();
        Quantity = Math.Clamp( quantity, 1, Definition.MaxStack );
    }

    public InventoryFootprint Footprint => Definition.GetFootprint( IsRotated );
    public float TotalWeight => Definition.UnitWeight * Quantity;
    public bool CanStackWith( InventoryItemStack other ) => other is not null && Definition.Id == other.Definition.Id;
    public int AvailableStackSpace => Math.Max( 0, Definition.MaxStack - Quantity );
    public bool IsFull => AvailableStackSpace <= 0;

    public void SetGridState( InventoryGridPosition position, bool rotated )
    {
        Position = position;
        IsRotated = rotated && Definition.CanRotate;
    }

    public bool TryAddQuantity( int amount, out int remainder )
    {
        if ( amount <= 0 )
        {
            remainder = amount;
            return false;
        }

        var accepted = Math.Min( amount, AvailableStackSpace );
        Quantity += accepted;
        remainder = amount - accepted;
        return accepted > 0;
    }

    public bool TryRemoveQuantity( int amount )
    {
        if ( amount <= 0 || amount > Quantity )
            return false;

        Quantity -= amount;
        return true;
    }
}
