
using Facepunch.Customization;

namespace Facepunch.Minigolf;

internal class CustomizationRenderScene : Panel
{
	private ScenePanel ScenePanel;
	private SceneWorld SceneWorld;
	private Angles CameraAngle;

	private int hash;

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		Build();
	}

	public override void Tick()
	{
		base.Tick();

		if ( SceneWorld.IsValid() )
		{
			var trail = SceneWorld.SceneObjects.FirstOrDefault( x => x is SceneParticles ) as SceneParticles;
			if ( trail.IsValid() )
			{
				trail.Simulate( RealTime.Delta );

				var pos = new Vector3( MathF.Cos( Time.Now * 4f ) * 12f, MathF.Sin( Time.Now * 4f ) * 8f, -5f );
				trail.SetControlPoint( 0, pos );
			}
		}

		var cc = Game.LocalClient.Components.Get<CustomizationComponent>();
		if ( cc == null ) return;

		var newhash = cc.GetItemHash();
		if ( newhash == hash ) return;

		hash = newhash;

		Build();
	}

	private void Build()
	{
		ScenePanel?.Delete();
		ScenePanel = null;

		SceneWorld?.Delete();
		SceneWorld = new SceneWorld();

		var modelscale = 3f;
		var golfball = new SceneModel( SceneWorld, "models/golf_ball.vmdl", Transform.Zero.WithScale( modelscale ) );

		ScenePanel = Add.ScenePanel( SceneWorld, Vector3.Zero, Rotation.From( CameraAngle ), 75 );
		ScenePanel.Style.Width = Length.Percent( 100 );
		ScenePanel.Style.Height = Length.Percent( 100 );
		ScenePanel.Camera.Position = golfball.Rotation.Forward * 32f + Vector3.Up * 3f;
		ScenePanel.Camera.Rotation = Rotation.LookAt( golfball.Rotation.Backward ).RotateAroundAxis( Vector3.Forward, -90 );
		CameraAngle = ScenePanel.Camera.Rotation.Angles();

		var sun = new SceneSunLight( SceneWorld, Rotation.FromPitch( 50 ), Color.White * 0.5f + Color.Cyan * 0.1f );
		sun.ShadowsEnabled = true;
		sun.SkyColor = Color.White * 0.15f + Color.Cyan * 0.25f;

		new SceneLight( SceneWorld, ScenePanel.Camera.Position + Vector3.Up * 5 + Vector3.Right * 2, 200, Color.White );
		new SceneLight( SceneWorld, Vector3.Down * 50 + Vector3.Left * 20, 200, Color.White.Darken( .25f ) );

		new SceneCubemap( SceneWorld, Texture.Load( "textures/cubemaps/default.vtex" ), BBox.FromPositionAndSize( Vector3.Zero, 1000 ) );

		var cc = Sandbox.Game.LocalClient.Components.Get<CustomizationComponent>();
		var skinItem = cc.GetEquippedItem( CustomizationItem.CategoryType.Skin );
		if ( skinItem != null && !string.IsNullOrEmpty( skinItem.SkinTexture ) )
		{
			var skin = Material.Load( skinItem.SkinTexture );
			golfball.SetMaterialOverride( skin );
		}

		var hatItem = cc.GetEquippedItem( CustomizationItem.CategoryType.Hat );
		if ( hatItem != null && !string.IsNullOrEmpty( hatItem.HatModel ) )
		{
			new SceneModel( SceneWorld, hatItem.HatModel, Transform.Zero.WithScale( modelscale ).WithPosition( Vector3.Up * 2.35f * modelscale ) );
		}

		var trailItem = cc.GetEquippedItem( CustomizationItem.CategoryType.Trail );
		if ( trailItem != null && !string.IsNullOrEmpty( trailItem.TrailParticle ) )
		{
			new SceneParticles( SceneWorld, trailItem.TrailParticle );
		}
	}

}
