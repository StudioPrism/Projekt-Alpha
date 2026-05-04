using Sandbox.UI;

public sealed class InventoryController : Component
{
    [Property] public string ToggleAction { get; set; } = "Score";
    [Property] public int GridWidth { get; set; } = 10;
    [Property] public int GridHeight { get; set; } = 6;
    [Property] public float MaxWeight { get; set; } = 30f;
    [Property] public bool SeedTestItems { get; set; } = true;

    public InventoryContainer Container { get; private set; }
    public bool IsOpen { get; private set; }
    public int UiVersion { get; private set; }

    private ScreenPanel _screen;
    private InventoryGridPanel _panel;

    protected override void OnStart()
    {
        Container = new InventoryContainer( "tactical_pack", "Tactical Pack", GridWidth, GridHeight, MaxWeight );

        if ( SeedTestItems )
            AddTestItems();

        _screen = new ScreenPanel();
        _panel = _screen.GetPanel().AddChild<InventoryGridPanel>();
        _panel.Owner = this;

        SetOpen( false );
    }

    protected override void OnUpdate()
    {
        if ( IsProxy ) return;

        if ( Input.Pressed( ToggleAction ) )
            SetOpen( !IsOpen );
    }

    protected override void OnDestroy()
    {
        _screen?.GetPanel()?.Delete( true );
        Input.EnableVirtualCursor = false;
    }

    public void SetOpen( bool open )
    {
        IsOpen = open;
        UiVersion++;

        Input.EnableVirtualCursor = open;
        _panel?.SetClass( "open", open );
        _panel?.SetClass( "closed", !open );
    }

    private void AddTestItems()
    {
        Container.TryPlaceItem( new InventoryItemStack( InventoryItemCatalog.Rifle556 ), new InventoryGridPosition( 0, 0 ) );
        Container.TryPlaceItem( new InventoryItemStack( InventoryItemCatalog.Pistol9Mm ), new InventoryGridPosition( 7, 0 ), true );
        Container.TryPlaceItem( new InventoryItemStack( InventoryItemCatalog.Ammo556Box, 60 ), new InventoryGridPosition( 0, 3 ) );
        Container.TryPlaceItem( new InventoryItemStack( InventoryItemCatalog.Ammo9MmLoose, 34 ), new InventoryGridPosition( 2, 3 ) );
        Container.TryPlaceItem( new InventoryItemStack( InventoryItemCatalog.Magazine9Mm ), new InventoryGridPosition( 3, 3 ) );
        Container.TryPlaceItem( new InventoryItemStack( InventoryItemCatalog.Medkit ), new InventoryGridPosition( 5, 3 ) );
        Container.TryPlaceItem( new InventoryItemStack( InventoryItemCatalog.WaterBottle ), new InventoryGridPosition( 8, 3 ) );
        Container.TryPlaceItem( new InventoryItemStack( InventoryItemCatalog.CannedFood, 2 ), new InventoryGridPosition( 9, 3 ) );
        Container.TryPlaceItem( new InventoryItemStack( InventoryItemCatalog.Bandage, 4 ), new InventoryGridPosition( 9, 4 ) );
    }
}
