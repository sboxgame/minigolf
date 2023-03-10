using Facepunch.Minigolf.Entities;

namespace Facepunch.Minigolf;

public class SpectatingInfo : Panel
{
	Label SpectatingLabel { get; set; }

	public SpectatingInfo()
	{
		StyleSheet.Load( "/UI/SpectatingInfo.scss" );
		SpectatingLabel = Add.Label( "Spectating" );
	}

	public override void Tick()
	{
		var camera = Game.Current.FollowBallCamera;
		if ( camera is null )
			return;

		var ball = Sandbox.Game.LocalClient as Ball;
		var target = camera.Target;
		SetClass( "show", target is not null && ball is null );

		if ( target is null )
			return;

		SpectatingLabel.Text = $"Spectating {target.Client.Name}";
	}
}
