namespace Facepunch.Minigolf.Entities.Desktop;

public class DesktopInputComponent : Ball.Component, IInputComponent
{
	[ClientInput] public float ShotPower { get; private set; }
	public float LastShotPower { get; private set; }

	[ClientInput] public Angles ViewAngles { get; set; }

	public override void BuildInput()
	{
		base.BuildInput();

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch is > 90f or < -90f )
			look = look.WithYaw( look.yaw * -1f );

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;

		// If we're in play, don't do anything.
		if ( Ball.InPlay )
			return;

		if ( Input.Down( InputActions.Attack1 ) )
		{
			var delta = Input.AnalogLook.pitch * RealTime.Delta;
			ShotPower = Math.Clamp( ShotPower - delta, 0, 1 );
		}
	}
}
