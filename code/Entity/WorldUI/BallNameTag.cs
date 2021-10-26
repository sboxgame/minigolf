using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Minigolf
{
	// TODO: Ideally I want the SceneObject to be parented to the ball and remain in the same position
	//       And then just offset the panel when rendering it.
	public partial class BallNameTag : WorldPanel
	{
		Ball Owner;

		public BallNameTag( Ball owner )
		{
			Owner = owner;

			Add.Image( $"avatar:{ owner.Client.SteamId }" );
			Add.Label( owner.Client.Name );

			StyleSheet.Load( "/Entity/WorldUI/BallNameTag.scss" );

			// Can't do this cause it'll just spin like nuts
			// owner.SceneObject.AddChild( "NameTagPanel", SceneObject );
		}

		[Event.PreRender]
		void MoveNameTag()
		{
			// Don't tidy up here, it's the owner's responsibility
			if ( !Owner.IsValid() )
				return;

			// TODO: Put on ground normal
			Position = Owner.Position;
			Position -= Vector3.Up * 2.9f;

			var forward = Angles.AngleVector( Owner.Direction );
			var right = Vector3.Cross( forward, Vector3.Up );

			Position += right * 8.0f;
			Rotation = Rotation.From( Owner.Direction ) * Rotation.FromYaw(180) * Rotation.From( Vector3.Up.EulerAngles );

			PanelBounds = new Rect( 0, -80, 800, 160 );
		}
	}
}
