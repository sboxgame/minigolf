using System.Text.Json;
using System.Text.Json.Serialization;

namespace Facepunch.Minigolf;

public struct BallCosmetics
{
	public DateTimeOffset SavedAt { get; set; }

	public List<GameObject> All { get; set; }

	[JsonIgnore]
	public List<Cosmetic> Current { get; set; }

	public BallCosmetics()
	{
		SavedAt = DateTimeOffset.UtcNow;
		All = new();
		Current = new();
	}
}

/// <summary>
/// Controls cosmetics for the player. This can potentially exist anywhere. It'll target a <see cref="ModelRenderer"/> and apply everything there.
/// It holds the saved cosmetics using <see cref="Cosmetics"/>, and can be saved using <see cref="SetSaved(List{Facepunch.Minigolf.CosmeticResource})"/>
/// </summary>
public partial class CosmeticController : Component
{
	/// <summary>
	/// The model we're editing
	/// </summary>
	[Property]
	public ModelRenderer Renderer { get; set; }

	/// <summary>
	/// Should we update the position?
	/// </summary>
	[Property]
	public bool Update { get; set; } = true;

	/// <summary>
	/// What's our cosmetic setup?
	/// </summary>
	[ConVar( "cosmetics" )]
	public static string Serialized { get; set; } = "{}";

	/// <summary>
	/// The current save
	/// </summary>
	[Property, JsonIgnore, InlineEditor]
	public BallCosmetics Cosmetics { get; set; }

	/// <summary>
	/// Store previous position so we can get the direction
	/// </summary>
	private Vector3 PreviousPosition { get; set; }

	/// <summary>
	/// The local cosmetic controller
	/// </summary>
	public static CosmeticController Local { get; set; }

	protected override void OnStart()
	{
		if ( !IsProxy )
		{
			Local = this;
			Cosmetics = Json.Deserialize<BallCosmetics>( Serialized );
		}

		if ( Update )
		{
			// We don't care about parented transforms for this, since the ball rolls around
			GameObject.Flags |= GameObjectFlags.Absolute;
		}

		TryLoad();
	}

	[Rpc.Broadcast]
	private void Share( string serialized )
	{
		var save = Json.Deserialize<BallCosmetics>( serialized );

		foreach ( var prefab in save.All )
		{
			Set( prefab, true );
		}
	}

	/// <summary>
	/// If we've got any local changes, re-setup everything
	/// </summary>
	private void Sync()
	{
		Clear();

		// Make sure we're synced
		foreach ( var prefab in Cosmetics.All )
		{
			Set( prefab, true, false );
		}
	}

	public void Preview( GameObject prefab, bool preview = true )
	{
		Sync();

		if ( preview )
		{
			Set( prefab, true, false );
		}
	}

	private void TryLoad()
	{
		if ( IsProxy )
			return;

		// Send the serialized set of ball cosmetics to everyone for this player, so it's networked
		Share( Serialized );
	}

	/// <summary>
	/// Enable or disable a cosmetic
	/// </summary>
	/// <param name="prefab"></param>
	/// <param name="active"></param>
	/// <param name="addToList"></param>
	public void Set( GameObject prefab, bool active = true, bool addToList = true )
	{
		var cosmetic = prefab.GetComponent<Cosmetic>();

		if ( addToList && !cosmetic.CanEquip() )
			return;
		
		var existing = Cosmetics.Current.FirstOrDefault( x => x.Title.Equals( cosmetic.Title ) );

		// Always remove existing cosmetic
		{
			if ( addToList )
				Cosmetics.All.Remove( prefab );

			if ( existing.IsValid() )
			{
				Cosmetics.Current.Remove( existing );
				existing.DestroyGameObject();
			}
		}

		if ( active )
		{
			if ( addToList )
				Cosmetics.All.Add( prefab );

			var go = prefab.Clone( new CloneConfig()
			{
				StartEnabled = true,
				Transform = new Transform(),
				Parent = GameObject,
			} );

			var cosmeticInst = go.GetComponent<Cosmetic>();
			if ( cosmeticInst.IsValid() )
			{
				Cosmetics.Current.Add( cosmeticInst );
			}
			else
			{
				Log.Warning( $"Tried to add a cosmetic item that has no Cosmetic component somehow." );
			}
		}
	}

	protected override void OnUpdate()
	{
		if ( !Update )
			return;

		WorldPosition = GameObject.Parent.WorldPosition;

		var dir = WorldPosition - PreviousPosition;
		PreviousPosition = WorldPosition;

		WorldRotation = Rotation.From( 0, Vector3.VectorAngle( dir ).yaw, 0 );
	}

	/// <summary>
	/// Save a list of cosmetics
	/// </summary>
	public static void Save()
	{
		Serialized = Json.Serialize( Local.Cosmetics );
	}

	public void Clear()
	{
		Cosmetics.Current
			.ForEach( x => x.DestroyGameObject() );

		Cosmetics.Current.Clear();
		Renderer.MaterialOverride = null;
	}

	/// <summary>
	/// Clear all cosmetics
	/// </summary>
	public static void ClearSaved()
	{
		if ( Local.IsValid() )
		{
			Local.Clear();
		}

		Local.Cosmetics = new BallCosmetics();
		Save();
	}
}
