using System;

public readonly struct InventoryFootprint : IEquatable<InventoryFootprint>
{
    public int Width { get; }
    public int Height { get; }

    public InventoryFootprint( int width, int height )
    {
        Width = Math.Max( 1, width );
        Height = Math.Max( 1, height );
    }

    public int Area => Width * Height;
    public InventoryFootprint Rotated => new InventoryFootprint( Height, Width );

    public bool Equals( InventoryFootprint other ) => Width == other.Width && Height == other.Height;
    public override bool Equals( object obj ) => obj is InventoryFootprint other && Equals( other );
    public override int GetHashCode() => HashCode.Combine( Width, Height );
    public override string ToString() => $"{Width}x{Height}";

    public static bool operator ==( InventoryFootprint left, InventoryFootprint right ) => left.Equals( right );
    public static bool operator !=( InventoryFootprint left, InventoryFootprint right ) => !left.Equals( right );
}
