using System;

public readonly struct InventoryGridPosition : IEquatable<InventoryGridPosition>
{
    public int X { get; }
    public int Y { get; }

    public InventoryGridPosition( int x, int y )
    {
        X = x;
        Y = y;
    }

    public bool Equals( InventoryGridPosition other ) => X == other.X && Y == other.Y;
    public override bool Equals( object obj ) => obj is InventoryGridPosition other && Equals( other );
    public override int GetHashCode() => HashCode.Combine( X, Y );
    public override string ToString() => $"{X},{Y}";

    public static bool operator ==( InventoryGridPosition left, InventoryGridPosition right ) => left.Equals( right );
    public static bool operator !=( InventoryGridPosition left, InventoryGridPosition right ) => !left.Equals( right );
}
