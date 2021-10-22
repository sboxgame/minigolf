
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

namespace Minigolf
{
	public partial class HoleMarker : Panel
	{
		private Image image;
		private Label distanceLabel;

		public HoleMarker()
		{
			StyleSheet.Load("/ui/HoleMarker.scss");

			image = Add.Image( "/ui/hole_marker.png" );
			distanceLabel = Add.Label( "64m" );
		}

		public override void Tick()
		{
			if ( Game.Current == null) return;
			var hole = Game.Current.Course.CurrentHole;

			if ( Local.Pawn is not Ball ball ) return;

			var distance = ball.Position.Distance( hole.GoalPosition + Vector3.Up * 8 ) * 0.02;

			distanceLabel.Text = $"{distance:0}m";

			var labelPos = hole.GoalPosition + Vector3.Up * 140; // go to 132 if zoomed in

			// Are we looking in this direction?
			// var lookDir = (labelPos - CurrentView.Position).Normal;
			// if ( CurrentView.Rotation.Forward.Dot( lookDir ) < 0.5 )
			// 	return;

			float dist = labelPos.Distance( CurrentView.Position );
			// var objectSize = 0.05f / dist / (2.0f * MathF.Tan( (CurrentView.FieldOfView / 2.0f).DegreeToRadian() )) * 3000.0f;
			var objectSize = 0.5f;

			objectSize = objectSize.Clamp( 0.25f, 0.5f );

			var screenPos = labelPos.ToScreen();

			Style.Left = Length.Fraction( screenPos.x );
			Style.Top = Length.Fraction( screenPos.y );
			Style.Opacity = 1;

			var transform = new PanelTransform();
			transform.AddTranslateY( Length.Fraction( -1.0f ) );
			transform.AddScale( objectSize );
			transform.AddTranslateX( Length.Fraction( -0.5f ) );

			Style.Transform = transform;
			Style.Dirty();
		}
	}

}
