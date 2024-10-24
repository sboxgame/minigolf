using Sandbox;
using System.Xml.Linq;

namespace Facepunch.Minigolf;

public struct BallCosmetics
{
	public DateTimeOffset SavedAt { get; set; }
	public Dictionary<string, CosmeticResource> All { get; set; }

	public BallCosmetics()
	{
		SavedAt = DateTimeOffset.UtcNow;
		All = new();
	}
}

/// <summary>
/// Controls cosmetics for the player. This can potentially exist anywhere. It'll target a <see cref="ModelRenderer"/> and apply everything there.
/// It holds the saved cosmetics using <see cref="Current"/>, and can be saved using <see cref="SetSaved(List{Facepunch.Minigolf.CosmeticResource})"/>
/// </summary>
public partial class CosmeticController : Component
{
	/// <summary>
	/// The model we're editing
	/// </summary>
	[Property]
	public ModelRenderer Renderer { get; set; }

	/// <summary>
	/// The particle system for trails
	/// </summary>
	[Property]
	public LegacyParticleSystem TrailSystem { get; set; }

	/// <summary>
	/// The default trail
	/// </summary>
	[Property]
	public ParticleSystem DefaultTrail { get; set; }

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
	public BallCosmetics Current { get; set; }

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
			Current = Json.Deserialize<BallCosmetics>( Serialized );
		}

		if ( Update )
		{
			// We don't care about parented transforms for this, since the ball rolls around
			GameObject.Flags |= GameObjectFlags.Absolute;
		}

		TryLoad();
	}

	[Broadcast]
	private void UpdateForEveryone( string serialized )
	{
		var save = Json.Deserialize<BallCosmetics>( serialized );

		foreach ( var resource in save.All )
		{
			Set( resource.Value, true );
		}
	}

	/// <summary>
	/// If we've got any local changes, re-setup everything
	/// </summary>
	private void Sync()
	{
		Clear();

		// Make sure we're synced
		foreach ( var resource in Current.All )
		{
			Set( resource.Value, true, false );
		}
	}

	public void Preview( CosmeticResource res, bool preview = true )
	{
		Sync();

		if ( preview )
		{
			Set( res, true, false );
		}
	}

	private void TryLoad()
	{
		if ( IsProxy )
			return;

		// Send the serialized set of ball cosmetics to everyone for this player, so it's networked
		UpdateForEveryone( Serialized );
	}

	/// <summary>
	/// Look for a cosmetic component that exists already
	/// </summary>
	/// <param name="resource"></param>
	/// <returns></returns>
	public CosmeticComponent Find( CosmeticResource resource )
	{
		return GetComponentsInChildren<CosmeticComponent>()
			.FirstOrDefault( x => x.Resource == resource );
	}

	/// <summary>
	/// Enable or disable a cosmetic
	/// </summary>
	/// <param name="resource"></param>
	/// <param name="active"></param>
	/// <param name="addToList"></param>
	public void Set( CosmeticResource resource, bool active = true, bool addToList = true )
	{
		var instance = Find( resource );

		// Nope.
		if ( addToList && !resource.CanEquip() )
			return;

		if ( !active )
		{
			if ( addToList )
				Current.All.Remove( resource.Category );

			if ( instance.IsValid() )
			{
				instance.GameObject.Destroy();
			}

			// Clear the skin
			if ( resource.Skin.IsValid() )
			{
				Renderer.MaterialOverride = null;
			}

			if ( resource.Trail.IsValid() && resource.Trail == TrailSystem.Particles )
			{
				TrailSystem.Particles = DefaultTrail;
			}

			return;
		}

		if ( instance.IsValid() )
		{
			instance.GameObject.Destroy();
		}

		if ( addToList )
			Current.All.Remove( resource.Category );

		if ( resource.Prefab.IsValid() )
		{
			var prefab = resource.Prefab.Clone( new CloneConfig()
			{
				Name = $"Ball Cosmetic ({resource.Title})",
				Parent = GameObject,
				Transform = new Transform(),
				StartEnabled = true,
			} );

			var component = prefab.GetComponent<CosmeticComponent>();
			if ( component.IsValid() )
			{
				component.Resource = resource;
			}
		}

		if ( resource.Skin.IsValid() )
		{
			Renderer.MaterialOverride = resource.Skin;
		}

		if ( resource.Trail.IsValid() )
		{
			TrailSystem.Particles = resource.Trail;
		}

		if ( addToList )
			Current.All.Add( resource.Category, resource );
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
		Serialized = Json.Serialize( Local.Current );
	}

	public void Clear()
	{
		GetComponentsInChildren<CosmeticComponent>()
			.ToList()
			.ForEach( x => x.GameObject.Destroy() );

		Renderer.MaterialOverride = null;
		TrailSystem.Particles = DefaultTrail;
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

		Local.Current = new BallCosmetics();
		Save();
	}
}
