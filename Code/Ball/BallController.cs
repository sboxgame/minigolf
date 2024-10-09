public sealed class BallController : Component
{
	[RequireComponent] 
	public Ball Ball { get; set; }

	[Property]
	public BallArrow Arrow { get; set; }

	[Property]
	public CameraController CameraController { get; set; }

	[Sync]
	public float ShotPower { get; set; } = 0f;

	/// <summary>
	/// Is the ball in play (is it moving, making it so we can't control it)
	/// </summary>
	[Sync, Change( nameof( OnInPlayChanged ) )] 
	public bool InPlay { get; set; }

	/// <summary>
	/// Called when the play mode of the ball changes, we're using this to post an event to the game manager
	/// </summary>
	/// <param name="before"></param>
	/// <param name="after"></param>
	private void OnInPlayChanged( bool before, bool after )
	{
		IGameEvent.Post( x => x.BallInPlay( Ball, after ) );
	}

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			CameraController.GameObject.Enabled = false;
			Arrow.GameObject.Enabled = false;
			return;
		}

		// Turn on the camera if we're in control
		CameraController.GameObject.Enabled = true;
		CameraController.GameObject.SetParent( null );
		CameraController.GameObject.Name = $"Camera (linked to {Ball.GameObject})";
		
		// Arrow shouldn't be parented since the ball goes mental
		Arrow.GameObject.SetParent( null );
	}

	protected override void OnDestroy()
	{
		if ( Arrow.IsValid() )
			Arrow.GameObject.Destroy();
	}

	void CheckInPlay()
	{
		// Sanity check, maybe our ball is hit by rotating blades?
		if ( !InPlay )
		{
			if ( Ball.Rigidbody.Velocity.Length >= 2.5f )
				InPlay = true;
		}

		// Check if our ball has pretty much stopped (waiting for 0 is nasty)
		if ( !Ball.Rigidbody.Velocity.Length.AlmostEqual( 0.0f, 2.5f ) )
			return;

		Ball.Rigidbody.Velocity = Vector3.Zero;
		InPlay = false;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		CheckInPlay();

		if ( InPlay || Ball.IsCupped )
		{
			Arrow.GameObject.Enabled = false;
			return;
		}

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
			if ( !Arrow.GameObject.Enabled )
				Arrow.GameObject.Enabled = true;

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
			InPlay = true;

			Ball.Stroke( Scene.Camera.WorldRotation.Yaw(), ShotPower );
			ShotPower = 0f;

			IGameEvent.Post( x => x.BallStroke( Ball ) );
		}
	}
}
