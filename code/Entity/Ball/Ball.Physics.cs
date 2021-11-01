using Sandbox;
using System;

namespace Minigolf
{
	public partial class Ball
	{
		protected Vector3 _velocity;
		public override Vector3 Velocity { get => _velocity; set => _velocity = value; }

		static float PowerMultiplier => 2500.0f;

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( IsServer )
			{
				using ( Prediction.Off() )
				{
					Move();
				}
			}
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
			mover.Trace = mover.Trace.Radius( 3.0f ).Ignore( this );
			mover.MaxStandableAngle = 50.0f;
			mover.GroundBounce = 0.25f; // TODO: Get from ground surface?
			mover.WallBounce = 0.5f;

			// Apply gravity
			mover.Velocity += Vector3.Down * 800 * Time.Delta;

			var groundTrace = mover.TraceDirection( Vector3.Down * 0.5f );
			if ( groundTrace.Hit && groundTrace.Normal.Angle( Vector3.Up ) < 1.0f )
			{
				mover.Velocity = ProjectOntoPlane( mover.Velocity, groundTrace.Normal );
			}

			mover.TryMove( Time.Delta );

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
			var soundName = $"minigolf.ball_impact_on_concrete{ Rand.Int( 1, 4 ) }";
			var sound = Sound.FromWorld( soundName, pos );
			sound.SetVolume( 0.2f + Math.Clamp( speed / 1250.0f, 0.0f, 0.8f ) );
			sound.SetPitch( 0.5f + Math.Clamp( speed / 1250.0f, 0.0f, 0.5f ) );

			var particle = Particles.Create( "particles/gameplay/ball_hit/ball_hit.vpcf", pos );
			particle.Destroy( false );
		}

		[Event.Tick.Server]
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
}
