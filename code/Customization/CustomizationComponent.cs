using System.Text.Json;

namespace Facepunch.Customization;

public class CustomizationComponent : EntityComponent
{
	public static string EnsembleJson
	{
		get => Cookie.Get( "customization.ensemble", string.Empty );
		set => Cookie.Set( "customization.ensemble", value );
	}

	public List<CustomizationItem> Items = new();

	protected override void OnActivate()
	{
		base.OnActivate();

		if ( Game.IsClient )
		{
			Deserialize( EnsembleJson );
		}
	}

	public CustomizationItem GetEquippedItem( CustomizationItem.CategoryType category )
	{
		return Items.FirstOrDefault( x => x.Category == category );
	}

	public void Equip( string resourceName ) => Equip( CustomizationItem.Find( resourceName ) );
	public void Equip( CustomizationItem item )
	{
		if ( item == null )
			throw new Exception( "Can't equip null" );

		if ( Items.Contains( item ) )
			return;

		var itemInSlot = GetEquippedItem( item.Category );
		if ( itemInSlot != null )
		{
			Unequip( itemInSlot );
		}

		Items.Add( item );

		if ( Game.IsClient )
		{
			EnsembleJson = Serialize();
			EquipItemOnServer( item.ResourceName );
		}
	}

	public void Unequip( string resourceName ) => Unequip( CustomizationItem.Find( resourceName ) );
	public void Unequip( CustomizationItem item )
	{
		if ( item == null )
			throw new Exception( "Can't equip null" );

		if ( !Items.Contains( item ) )
			return;

		Items.Remove( item );

		if ( Game.IsClient )
		{
			EnsembleJson = Serialize();
			UnequipItemOnServer( item.ResourceName );
		}
	}

	public bool IsEquipped( CustomizationItem item )
	{
		if ( item == null ) throw new Exception( "Can't equip null" );

		return Items.Any( x => x.ResourceName == item.ResourceName );
	}

	public string Serialize()
	{
		return JsonSerializer.Serialize( Items.Select( x => new Entry { ResourceName = x.ResourceName } ) );
	}

	public void Deserialize( string json )
	{
		Items.Clear();

		if ( string.IsNullOrWhiteSpace( json ) )
			return;

		try
		{
			var entries = JsonSerializer.Deserialize<Entry[]>( json );

			foreach ( var entry in entries )
			{
				var item = CustomizationItem.Find( entry.ResourceName );
				if ( item == null ) continue;
				Equip( item );
			}
		}
		catch ( Exception e )
		{
			Log.Warning( e, "Error deserailizing clothing" );
		}
	}

	public int GetItemHash()
	{
		int hash = Items.GetHashCode();
		foreach ( var item in Items )
		{
			hash = HashCode.Combine( hash, item.ResourceName );
		}
		return hash;
	}

	public struct Entry
	{
		public string ResourceName { get; set; }
	}

	[ConCmd.Server]
	public static void EquipItemOnServer( string resourceName )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		var cfg = caller.Components.Get<CustomizationComponent>();
		if ( cfg == null ) return;

		cfg.Equip( resourceName );
	}

	[ConCmd.Server]
	public static void UnequipItemOnServer( string resourceName )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		var cfg = caller.Components.Get<CustomizationComponent>();
		if ( cfg == null ) return;

		cfg.Unequip( resourceName );
	}
}
