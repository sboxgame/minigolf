using Sandbox;
using System;

namespace Facepunch.Minigolf.Entities;

public partial class Ball
{
	/// <summary>
	/// The current shot power...
	/// </summary>
	public float ShotPower { get; set; } = 0.0f;
	public float LastShotPower { get; set; } = 0.0f;

	[ClientInput] public Angles ViewAngles { get; set; }

	public override void BuildInput()
	{
		var look = Input.AnalogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;

		// If we're in play, don't do anything.
		if ( InPlay )
			return;

		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			float delta = Input.AnalogLook.pitch * RealTime.Delta;
			ShotPower = Math.Clamp( ShotPower - delta, 0, 1 );
		}

		if ( ShotPower >= 0.01f && !Input.Down( InputButton.PrimaryAttack ) )
		{
			Game.Stroke( Sandbox.Camera.Rotation.Yaw(), ShotPower );
			LastShotPower = ShotPower;
			ShotPower = 0;
		}
	}
}
