using Facepunch.Customization;

namespace Facepunch.Minigolf.Entities;

public partial class Ball
{
	[Net]
	public AnimatedEntity Hat { get; set; }
	private AnimatedEntity localhat;

	private Vector3 prevPosition;

	private int itemHash = -1;
	private Particles trail;

	private void CleanupCustomization()
	{
		if ( Game.IsServer )
		{
			if ( Hat.IsValid() ) Hat.Delete();
			if ( trail != null ) trail.Destroy();
		}

		if ( Sandbox.Game.IsClient )
		{
			if ( localhat.IsValid() ) localhat.Delete();
		}

		Hat = null;
		trail = null;
		localhat = null;
	}

	[GameEvent.Tick.Server]
	private void EnsureCustomization()
	{
		var cc = Client.Components.GetOrCreate<CustomizationComponent>();

		var hash = cc.GetItemHash();
		if ( hash == itemHash ) return;

		itemHash = hash;
		ApplyCustomization();
	}

	private void MoveHat()
	{
		if ( !Hat.IsValid() )
			return;

		if ( IsLocalPawn && !localhat.IsValid() )
		{
			Hat.RenderColor = Color.Transparent;
			localhat = new AnimatedEntity( Hat.GetModelName() );
		}

		var target = IsLocalPawn ? localhat : Hat;
		target.Position = Position + Vector3.Up * 2;

		var dir = Position - prevPosition;
		prevPosition = Position;

		if ( dir.IsNearlyZero() )
			return;

		var targetAngles = Vector3.VectorAngle( dir );
		target.Rotation = Rotation.Lerp( target.Rotation, Rotation.From( targetAngles ), 5f * Time.Delta );
	}

	private void ApplyCustomization()
	{
		var cc = Client.Components.GetOrCreate<CustomizationComponent>();

		CleanupCustomization();
		CleanupCustomizationOnClient();

		var trailItem = cc.GetEquippedItem( CustomizationItem.CategoryType.Trail );
		if ( trailItem != null )
		{
			trail = Particles.Create( trailItem.TrailParticle, this );
			trail.SetPosition( 1, Vector3.One ); // Color
		}

		var hatItem = cc.GetEquippedItem( CustomizationItem.CategoryType.Hat );
		if ( hatItem != null )
		{
			Hat = new AnimatedEntity( hatItem.HatModel );
		}

		var skinItem = cc.GetEquippedItem( CustomizationItem.CategoryType.Skin );
		SetSkinOnClient( To.Everyone, NetworkIdent, skinItem is not null ? skinItem.SkinTexture : defaultSkin );
	}

	[ClientRpc]
	private void CleanupCustomizationOnClient()
	{
		CleanupCustomization();
	}

	[ConCmd.Client( "setskin", CanBeCalledFromServer = true )]
	public static void SetSkinOnClient( int ballIdent, string assetPath )
	{
		var ball = Entity.FindByIndex( ballIdent );
		if ( ball is not Ball b )
			return;

		b.SetMaterialOverride( assetPath );
	}
}
