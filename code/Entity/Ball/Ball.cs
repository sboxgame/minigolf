﻿namespace Facepunch.Minigolf.Entities;

public partial class Ball : ModelEntity
{
	[ConVar.Server( "minigolf_ball_debug" )]
	public static bool Debug { get; set; } = false;
	[Net] public bool InPlay { get; set; } = false;
	[Net] public bool Cupped { get; set; } = false;
	[Net] public Angles Direction { get; set; }
	public Vector3 LastPosition { get; set; }
	public Angles LastAngles { get; set; }

	static readonly Model GolfBallModel = Model.Load( "models/golf_ball.vmdl" );

	public bool InWater = false;

	[BindComponent] public FollowBallCamera Camera { get; }

	public override void Spawn()
	{
		base.Spawn();

		Model = GolfBallModel;

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );

		EnableTraceAndQueries = false;

		Transmit = TransmitType.Always;

		Predictable = true;

		Tags.Add( "golf_ball" );
	}
	Glow glow;

	public override void ClientSpawn()
	{
		Components.Create<FollowBallCamera>();

		base.ClientSpawn();
		CreateParticles();

		glow = Components.GetOrCreate<Glow>();

		glow.Enabled = true;
		glow.Width = 0.25f;
		glow.Color = new Color( 0.0f, 0.0f, 0.0f, 0.0f );
		glow.ObscuredColor = Color.Transparent;
		glow.InsideObscuredColor = Color.Black.WithAlpha( 0.72f );


	}

	public void Cup( bool holeInOne = false )
	{
		if ( Cupped ) return;

		Cupped = true;

		var sound = PlaySound( "minigolf.sink_into_cup" );
		sound.SetVolume( 1.0f );
		sound.SetPitch( Sandbox.Game.Random.Float( 0.75f, 1.25f ) );
	}

	public void ResetPosition( Vector3 position, Angles direction )
	{
		Position = position;
		Velocity = Vector3.Zero;
		ResetInterpolation();

		InPlay = false;
		Cupped = false;
		InWater = false;

		Direction = direction;

		// Tell the player we reset the ball
		PlayerResetPosition( To.Single( this ), position, direction );
	}

	[ClientRpc]
	protected void PlayerResetPosition( Vector3 position, Angles angles )
	{
		Camera.TargetAngles = new( 14, angles.yaw, 0 );
		//Camera.Rotation = Rotation.From( 14, angles.yaw, 0 );
	}

}
