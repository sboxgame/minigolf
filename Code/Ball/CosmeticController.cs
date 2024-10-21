using System.Collections.Generic;

namespace Facepunch.Minigolf;

public partial class CosmeticController : Component
{
	/// <summary>
	/// The model we're editing
	/// </summary>
	[Property]
	public ModelRenderer Renderer { get; set; }

	/// <summary>
	/// A list of resources we'll spawn by default, for testing purposes.
	/// </summary>
	[Property]
	public List<CosmeticResource> InitialResources { get; set; }

	/// <summary>
	/// Should we update the position?
	/// </summary>
	[Property]
	public bool Update { get; set; } = true;

	/// <summary>
	/// What's our cosmetic setup?
	/// </summary>
	[ConVar( "cosmetics" )]
	public static string Saved { get; set; } = "[]";

	public static void SetSaved( List<CosmeticResource> resources )
	{
		Saved = Json.Serialize( resources );
	}

	public static void ClearSaved()
	{
		Saved = "[]";
	}

	/// <summary>
	/// Store previous position so we can get the direction
	/// </summary>
	private Vector3 PreviousPosition { get; set; }

	public static CosmeticController Local { get; set; }

	protected override void OnStart()
	{
		if ( !IsProxy )
			Local = this;

		if ( Update )
		{
			// We don't care about parented transforms for this, since the ball rolls around
			GameObject.Flags |= GameObjectFlags.Absolute;
		}

		try
		{
			var listOf = Json.Deserialize<List<CosmeticResource>>( Saved );

			foreach ( var resource in listOf )
			{
				Set( resource, true );
			}
		}
		catch
		{

		}
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
	public void Set( CosmeticResource resource, bool active = true )
	{
		if ( !active )
		{
			var instance = Find( resource );
			Log.Info( $"active instance: {instance}" );

			if ( instance.IsValid() )
			{
				instance.GameObject.Destroy();
			}

			// Clear the skin
			if ( resource.Skin.IsValid() )
			{
				Renderer.MaterialOverride = null;
			}

			return;
		}

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
				Log.Info( $"Let's set the resource to {resource}" );
			}
		}

		if ( resource.Skin.IsValid() )
		{
			Renderer.MaterialOverride = resource.Skin;
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
}
