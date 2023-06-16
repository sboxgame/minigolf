namespace Facepunch.Minigolf.Entities.Desktop;

public class ArrowComponent : Ball.Component
{
	[Ball.ComponentDependency] private DesktopInputComponent DesktopInput { get; set; }
	private PowerArrow PowerArrow { get; set; }

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( Ball.IsLocalPawn )
			AdjustArrow();
	}

	private void AdjustArrow()
	{
		// Only show the arrow if we're charging a shot, delete otherwise.
		if ( DesktopInput.ShotPower.AlmostEqual( 0 ) )
		{
			if ( PowerArrow == null )
				return;

			PowerArrow.Delete();
			PowerArrow = null;

			return;
		}

		if ( !PowerArrow.IsValid() )
			PowerArrow = new PowerArrow();

		var direction = Angles.AngleVector( new Angles( 0, Sandbox.Camera.Rotation.Yaw(), 0 ) );

		var ballRadius = Ball.CollisionBounds.Size.z / 2;
		PowerArrow.Position = Position + Vector3.Down * ballRadius + Vector3.Up * 0.01f + direction * 5.0f;
		PowerArrow.Direction = direction;
		PowerArrow.Power = DesktopInput.ShotPower;
	}
}
