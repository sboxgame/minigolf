public sealed class BallController : Component
{
	[RequireComponent] 
	public Ball Ball { get; set; }

	[Property]
	public BallArrow Arrow { get; set; }

	[Sync]
	public float ShotPower { get; set; } = 0f;

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		var direction = Angles.AngleVector( new Angles( 0, Scene.Camera.WorldRotation.Yaw(), 0 ) );
		var ballRadius = Ball.Rigidbody.PhysicsBody.GetBounds().Size.z / 2;

		if ( Input.Pressed( "Attack1" ) )
		{
			Arrow.GameObject.Enabled = true;
		}
		if ( Input.Released( "Attack1" ) )
		{
			Arrow.GameObject.Enabled = false;
		}

		if ( Input.Down( "Attack1" ) )
		{
			float delta = Input.AnalogLook.pitch * RealTime.Delta;
			ShotPower = Math.Clamp( ShotPower - delta, 0, 1 );

			var color = ColorConvert.HSLToRGB( 120 - (int)(ShotPower * ShotPower * 120), 1.0f, 0.5f );
			Arrow.WorldPosition = WorldPosition + Vector3.Down * ballRadius + Vector3.Up * 0.01f + direction * 7.0f;
			Arrow.Renderer.Tint = color;
			// Build mesh, this sucks I think
			Arrow.Build( direction, ShotPower );
		}

		if ( ShotPower > 0.0f && Input.Released( "Attack1" ) )
		{
			Ball.Stroke( Scene.Camera.WorldRotation.Yaw(), ShotPower );
			ShotPower = 0f;
		}
	}
}
