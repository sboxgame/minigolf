using Sandbox;
using System;

namespace Facepunch.Minigolf.Entities;

public partial class Ball
{
	protected Vector3 _velocity;
	public override Vector3 Velocity { get => _velocity; set => _velocity = value; }

	static float PowerMultiplier => 2500.0f;

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( Sandbox.Game.IsServer )
		{
			using ( Prediction.Off() )
			{
				Move();
			}
		}

		MoveHat();
	}

	static Vector3 ProjectOntoPlane( Vector3 v, Vector3 normal, float overBounce = 1.0f )
	{
		float backoff = v.Dot( normal );

		if ( overBounce != 1.0 )
		{
			if ( backoff < 0 )
			{
				backoff *= overBounce;
			}
			else
			{
				backoff /= overBounce;
			}
		}

		return v - backoff * normal;
	}

	public virtual void Move()
	{
		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Radius( 1.5f ).Ignore( this );
		mover.MaxStandableAngle = 50.0f;
		mover.GroundBounce = 0.25f; // TODO: Get from ground surface?
		mover.WallBounce = 0.5f;

		var groundTrace = mover.TraceDirection( Vector3.Down * 0.5f );

		if ( groundTrace.Entity.IsValid() )
		{
			mover.GroundVelocity = groundTrace.Entity.Velocity;
		}

		// Apply gravity
		mover.Velocity += Vector3.Down * 800 * Time.Delta;

		if ( groundTrace.Hit && groundTrace.Normal.Angle( Vector3.Up ) < 1.0f )
		{
			mover.Velocity = ProjectOntoPlane( mover.Velocity, groundTrace.Normal );
		}

		mover.TryMove( Time.Delta );
		mover.TryUnstuck();

		if ( InWater )
		{
			mover.ApplyFriction( 5.0f, Time.Delta );
		}

		// Apply friction based on our ground surface
		if ( groundTrace.Hit )
		{
			var friction = groundTrace.Surface.Friction;

			// Apply more friction if the ball is close to stopping
			if ( mover.Velocity.Length < 1.0f )
				friction = 5.0f;

			mover.ApplyFriction( friction, Time.Delta );
		}
		else
		{
			// Air drag
			mover.ApplyFriction( 0.5f, Time.Delta );
		}

		Position = mover.Position;
		BaseVelocity = mover.GroundVelocity;
		Velocity = mover.Velocity;

		// Rotate the ball
		if ( Velocity.LengthSquared > 0.0f )
		{
			var dir = Velocity.Normal;
			var axis = new Vector3( -dir.y, dir.x, 0.0f );
			var angle = (Velocity.Length * Time.Delta) / (3.0f * (float)Math.PI);
			Rotation = Rotation.FromAxis( axis, 180.0f * angle ) * Rotation;
		}

		if ( mover.HitWall )
		{
			ImpactEffects( mover.Position, mover.Velocity.Length );
		}

		if ( Velocity.Length > 16.0f )
			Direction = Velocity.Normal.WithZ( 0 ).EulerAngles;
	}

	void ImpactEffects( Vector3 pos, float speed )
	{
		// Collision sound happens at this point, not entity
		var tr = Trace.Ray( pos, pos + Rotation.Forward * 20 )
		.Radius( 1 )
		.Ignore( this )
		.Run();

		var soundName = tr.Surface.Sounds.ImpactHard;
		var sound = Sound.FromWorld( soundName, pos );
		sound.SetVolume( 1f + Math.Clamp( speed / 1250.0f, 0.0f, 0.8f ) );
		sound.SetPitch( 1f + Math.Clamp( speed / 1250.0f, 0.0f, 0.5f ) );

		var particle = Particles.Create( "particles/gameplay/ball_hit/ball_hit.vpcf", pos );
		particle.Destroy( false );
	}

	[GameEvent.Tick.Server]
	protected void CheckInPlay()
	{
		// InPlay = false;
		// return;

		// Sanity check, maybe our ball is hit by rotating blades?
		if ( !InPlay )
		{
			if ( Velocity.Length >= 2.5f )
				InPlay = true;
		}

		// Check if our ball has pretty much stopped (waiting for 0 is nasty)
		if ( !Velocity.Length.AlmostEqual( 0.0f, 2.5f ) )
			return;

		Velocity = Vector3.Zero;
		InPlay = false;

		// Lets call something on the Game? (Maybe an RPC too?)
		// Delete effects? (They are clientside though)
	}
}
