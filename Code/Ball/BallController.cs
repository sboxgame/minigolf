using Facepunch.Minigolf;

public sealed class BallController : Component
{
	[RequireComponent] 
	public Ball Ball { get; set; }

	[Property]
	public BallArrow Arrow { get; set; }

	[Sync]
	public float ShotPower { get; set; } = 0f;

	[Property]
	public CameraComponent Camera { get; set; }

	[Sync]
	public float LastShotPower { get; set; } = 0f;

	/// <summary>
	/// Is the ball in play (is it moving, making it so we can't control it)
	/// </summary>
	[Sync, Change( nameof( OnInPlayChanged ) )] 
	public bool InPlay { get; set; }

	/// <summary>
	/// An object we'll place under the player's ball while they're in play.
	/// </summary>
	[Property]
	public LegacyParticleSystem InPlayObject { get; set; }

	/// <summary>
	/// Gets the active camera
	/// </summary>
	/// <returns></returns>
	public BaseCamera GetActiveCamera()
	{
		if ( Facepunch.Minigolf.GameManager.Instance.State != GameState.InPlay )
			return GetComponent<CinematicCamera>();

		if ( WantsFreeCamera() ) return GetComponent<FreeCamera>();
		return GetComponent<BallCamera>();
	}

	private bool wantsFreeCam = false;
	private bool WantsFreeCamera()
	{
		if ( InPlay ) return false;
		if ( Ball.IsCupped ) return false;
		return wantsFreeCam;
	}

	private void UpdateStatus()
	{
		var ballRadius = Ball.Rigidbody.PhysicsBody.GetBounds().Size.z / 2;
		InPlayObject.WorldPosition = GameObject.WorldPosition + Vector3.Down * ballRadius;
	}

	private BaseCamera CurrentCamera { get; set; }
	private void UpdateCamera()
	{
		if ( Input.Pressed( "View" ) )
		{
			wantsFreeCam = !wantsFreeCam;
		}

		var last = CurrentCamera;
		var cam = GetActiveCamera();
		CurrentCamera = cam;

		// Tick only the active camera
		if ( cam.IsValid() )
		{
			if ( last != cam )
			{
				cam.OnCameraActive();

				if ( last.IsValid() )
					last.OnCameraInactive();
			}

			cam.OnCameraUpdate();
		}
	}

	/// <summary>
	/// Called when the play mode of the ball changes, we're using this to post an event to the game manager
	/// </summary>
	/// <param name="before"></param>
	/// <param name="after"></param>
	private void OnInPlayChanged( bool before, bool after )
	{
		IGameEvent.Post( x => x.BallInPlay( Ball, after ) );

		InPlayObject.Enabled = !after;
	}

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			Camera.GameObject.Enabled = false;
			Arrow.GameObject.Enabled = false;
			return;
		}

		// Turn on the camera if we're in control
		Camera.GameObject.Enabled = true;
		Camera.GameObject.SetParent( null );
		Camera.GameObject.Name = $"Camera (linked to {Ball.GameObject})";
		
		// Arrow shouldn't be parented since the ball goes mental
		Arrow.GameObject.SetParent( null );

		InPlayObject.GameObject.Flags |= GameObjectFlags.Absolute;
	}

	protected override void OnDestroy()
	{
		if ( Arrow.IsValid() )
			Arrow.GameObject.Destroy();
	}

	void CheckInPlay()
	{
		if ( InPlayObject.Enabled )
			InPlayObject?.SceneObject?.SetControlPoint( 0, WorldPosition );

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

		UpdateCamera();
		CheckInPlay();
		UpdateStatus();

		// We're not in play, so don't let us do anything
		if ( Facepunch.Minigolf.GameManager.Instance.State != GameState.InPlay )
			return;

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

			LastShotPower = ShotPower;
			ShotPower = 0f;

			IGameEvent.Post( x => x.BallStroke( Ball ) );
		}
	}
}
