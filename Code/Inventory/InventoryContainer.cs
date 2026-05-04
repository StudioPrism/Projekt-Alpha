using System;

public sealed class InventoryContainer
{
    private readonly List<InventoryItemStack> _items = new();

    public string Id { get; }
    public string DisplayName { get; }
    public int Width { get; }
    public int Height { get; }
    public float MaxWeight { get; }
    public IReadOnlyList<InventoryItemStack> Items => _items;

    public InventoryContainer( string id, string displayName, int width, int height, float maxWeight = 0f )
    {
        Id = id;
        DisplayName = displayName;
        Width = Math.Max( 1, width );
        Height = Math.Max( 1, height );
        MaxWeight = Math.Max( 0f, maxWeight );
    }

    public float CurrentWeight => _items.Sum( item => item.TotalWeight );
    public bool HasWeightLimit => MaxWeight > 0f;

    public InventoryPlacementResult TryPlaceItem( InventoryItemStack item, InventoryGridPosition position, bool rotated = false )
    {
        if ( item is null ) return InventoryPlacementResult.NullItem;
        if ( Contains( item ) ) return InventoryPlacementResult.ItemAlreadyPresent;
        if ( HasWeightLimit && CurrentWeight + item.TotalWeight > MaxWeight ) return InventoryPlacementResult.TooHeavy;
        if ( !IsFootprintInBounds( item.Definition.GetFootprint( rotated ), position ) ) return InventoryPlacementResult.OutOfBounds;
        if ( IsFootprintOccupied( item.Definition.GetFootprint( rotated ), position ) ) return InventoryPlacementResult.Occupied;

        item.SetGridState( position, rotated );
        _items.Add( item );
        return InventoryPlacementResult.Success;
    }

    public InventoryPlacementResult TryAutoPlaceItem( InventoryItemStack item, bool allowRotation = true )
    {
        if ( item is null ) return InventoryPlacementResult.NullItem;
        if ( Contains( item ) ) return InventoryPlacementResult.ItemAlreadyPresent;
        if ( HasWeightLimit && CurrentWeight + item.TotalWeight > MaxWeight ) return InventoryPlacementResult.TooHeavy;

        if ( TryFindSpace( item, allowRotation, out var position, out var rotated ) )
            return TryPlaceItem( item, position, rotated );

        return InventoryPlacementResult.NoSpace;
    }

    public InventoryPlacementResult TryMoveItem( InventoryItemStack item, InventoryGridPosition newPosition, bool? rotated = null )
    {
        if ( item is null ) return InventoryPlacementResult.NullItem;
        if ( !Contains( item ) ) return InventoryPlacementResult.ItemNotFound;

        var desiredRotation = rotated ?? item.IsRotated;
        var footprint = item.Definition.GetFootprint( desiredRotation );

        if ( !IsFootprintInBounds( footprint, newPosition ) ) return InventoryPlacementResult.OutOfBounds;
        if ( IsFootprintOccupied( footprint, newPosition, item ) ) return InventoryPlacementResult.Occupied;

        item.SetGridState( newPosition, desiredRotation );
        return InventoryPlacementResult.Success;
    }

    public InventoryPlacementResult TryRotateItem( InventoryItemStack item )
    {
        if ( item is null ) return InventoryPlacementResult.NullItem;
        if ( !Contains( item ) ) return InventoryPlacementResult.ItemNotFound;
        if ( !item.Definition.CanRotate ) return InventoryPlacementResult.OutOfBounds;

        return TryMoveItem( item, item.Position, !item.IsRotated );
    }

    public InventoryPlacementResult TryStackItem( InventoryItemStack source, InventoryItemStack target )
    {
        if ( source is null || target is null ) return InventoryPlacementResult.NullItem;
        if ( !Contains( source ) || !Contains( target ) ) return InventoryPlacementResult.ItemNotFound;
        if ( source == target ) return InventoryPlacementResult.NotStackable;
        if ( !target.CanStackWith( source ) ) return InventoryPlacementResult.NotStackable;
        if ( target.IsFull ) return InventoryPlacementResult.StackFull;

        target.TryAddQuantity( source.Quantity, out var remainder );

        if ( remainder <= 0 )
            _items.Remove( source );
        else
            source.TryRemoveQuantity( source.Quantity - remainder );

        return InventoryPlacementResult.Success;
    }

    public InventoryPlacementResult TrySplitStack( InventoryItemStack source, int quantity, InventoryGridPosition position, out InventoryItemStack splitStack, bool rotated = false )
    {
        splitStack = null;

        if ( source is null ) return InventoryPlacementResult.NullItem;
        if ( !Contains( source ) ) return InventoryPlacementResult.ItemNotFound;
        if ( quantity <= 0 || quantity >= source.Quantity ) return InventoryPlacementResult.InvalidQuantity;

        var candidate = new InventoryItemStack( source.Definition, quantity );
        var result = TryPlaceItem( candidate, position, rotated );

        if ( result != InventoryPlacementResult.Success )
            return result;

        source.TryRemoveQuantity( quantity );
        splitStack = candidate;
        return InventoryPlacementResult.Success;
    }

    public InventoryPlacementResult TryTransferItemTo( InventoryItemStack item, InventoryContainer destination, InventoryGridPosition position, bool rotated = false )
    {
        if ( item is null ) return InventoryPlacementResult.NullItem;
        if ( destination is null ) return InventoryPlacementResult.NullItem;
        if ( !Contains( item ) ) return InventoryPlacementResult.ItemNotFound;

        _items.Remove( item );
        var result = destination.TryPlaceItem( item, position, rotated );

        if ( result == InventoryPlacementResult.Success )
            return result;

        _items.Add( item );
        return result;
    }

    public InventoryPlacementResult TryAutoTransferItemTo( InventoryItemStack item, InventoryContainer destination, bool allowRotation = true )
    {
        if ( item is null ) return InventoryPlacementResult.NullItem;
        if ( destination is null ) return InventoryPlacementResult.NullItem;
        if ( !Contains( item ) ) return InventoryPlacementResult.ItemNotFound;

        _items.Remove( item );
        var result = destination.TryAutoPlaceItem( item, allowRotation );

        if ( result == InventoryPlacementResult.Success )
            return result;

        _items.Add( item );
        return result;
    }

    public bool RemoveItem( InventoryItemStack item )
    {
        return item is not null && _items.Remove( item );
    }

    public bool Contains( InventoryItemStack item ) => item is not null && _items.Contains( item );

    public InventoryItemStack GetItemAt( InventoryGridPosition position )
    {
        return _items.FirstOrDefault( item => EnumerateCells( item.Position, item.Footprint ).Contains( position ) );
    }

    public bool TryFindSpace( InventoryItemStack item, bool allowRotation, out InventoryGridPosition position, out bool rotated )
    {
        position = default;
        rotated = false;

        if ( item is null )
            return false;

        var rotations = allowRotation && item.Definition.CanRotate
            ? new[] { false, true }
            : new[] { false };

        foreach ( var rotation in rotations )
        {
            var footprint = item.Definition.GetFootprint( rotation );

            for ( var y = 0; y <= Height - footprint.Height; y++ )
            {
                for ( var x = 0; x <= Width - footprint.Width; x++ )
                {
                    var candidate = new InventoryGridPosition( x, y );

                    if ( IsFootprintOccupied( footprint, candidate ) )
                        continue;

                    position = candidate;
                    rotated = rotation;
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsFootprintInBounds( InventoryFootprint footprint, InventoryGridPosition position )
    {
        if ( position.X < 0 || position.Y < 0 ) return false;
        if ( position.X + footprint.Width > Width ) return false;
        if ( position.Y + footprint.Height > Height ) return false;
        return true;
    }

    public bool IsFootprintOccupied( InventoryFootprint footprint, InventoryGridPosition position, InventoryItemStack ignoreItem = null )
    {
        foreach ( var cell in EnumerateCells( position, footprint ) )
        {
            if ( GetItemAt( cell ) is { } occupant && occupant != ignoreItem )
                return true;
        }

        return false;
    }

    public IEnumerable<InventoryGridPosition> EnumerateOccupiedCells()
    {
        foreach ( var item in _items )
        {
            foreach ( var cell in EnumerateCells( item.Position, item.Footprint ) )
                yield return cell;
        }
    }

    public static IEnumerable<InventoryGridPosition> EnumerateCells( InventoryGridPosition position, InventoryFootprint footprint )
    {
        for ( var y = 0; y < footprint.Height; y++ )
        {
            for ( var x = 0; x < footprint.Width; x++ )
                yield return new InventoryGridPosition( position.X + x, position.Y + y );
        }
    }
}
