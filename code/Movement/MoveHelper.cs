using Sandbox;

namespace Minigolf
{
	public struct MoveHelper
	{
		public Vector3 Position;
		public Vector3 Velocity;

		public bool HitWall;
		public Entity WallEntity;
		public Vector3 HitWallPos;

		public float GroundBounce;
		public float WallBounce;
		public float MaxStandableAngle;
		public Trace Trace;

		public MoveHelper( Vector3 position, Vector3 velocity ) : this()
		{
			Velocity = velocity;
			Position = position;
			GroundBounce = 0.0f;
			WallBounce = 0.0f;
			MaxStandableAngle = 10.0f;

			// Hit everything but other balls
			Trace = Trace.Ray( 0, 0 )
				.WorldAndEntities()
				.WithoutTags( "golf_ball" );
		}

		public TraceResult TraceFromTo( Vector3 start, Vector3 end )
		{
			return Trace.FromTo( start, end ).Run();
		}

		public TraceResult TraceDirection( Vector3 down )
		{
			return TraceFromTo( Position, Position + down );
		}

		public float TryMove( float timestep )
		{
			var timeLeft = timestep;
			float travelFraction = 0;
			HitWall = false;
			WallEntity = null;
			HitWallPos = Vector3.Zero;

			using var moveplanes = new VelocityClipPlanes( Velocity );

			for ( int bump = 0; bump < moveplanes.Max; bump++ )
			{
				if ( Velocity.Length.AlmostEqual( 0.0f ) )
					break;

				var pm = TraceFromTo( Position, Position + Velocity * timeLeft );

				if ( pm.StartedSolid )
				{
					Position += pm.Normal * 0.01f;

					continue;
				}

				travelFraction += pm.Fraction;

				if ( pm.Fraction > 0.0f )
				{
					Position = pm.EndPos + pm.Normal * 0.01f;

					moveplanes.StartBump( Velocity );
				}

				if ( !HitWall && !pm.StartedSolid && pm.Hit && pm.Normal.Angle( Vector3.Up ) >= MaxStandableAngle )
				{
					HitWall = true;
					WallEntity = pm.Entity;
					HitWallPos = pm.EndPos;
				}

				timeLeft -= timeLeft * pm.Fraction;

				if ( !moveplanes.TryAdd( pm.Normal, ref Velocity, IsFloor( pm ) ? GroundBounce : WallBounce ) )
					break;
			}

			if ( travelFraction == 0 )
				Velocity = 0;

			return travelFraction;
		}

		public bool IsFloor( TraceResult tr )
		{
			if ( !tr.Hit ) return false;
			return tr.Normal.Angle( Vector3.Up ) < MaxStandableAngle;
		}

		public void ApplyFriction( float frictionAmount, float delta )
		{
			float StopSpeed = 100.0f;

			var speed = Velocity.Length;
			if ( speed < 0.1f )
			{
				Velocity = 0;
				return;
			}

			// Bleed off some speed, but if we have less than the bleed
			//  threshold, bleed the threshold amount.
			float control = (speed < StopSpeed) ? StopSpeed : speed;

			// Add the amount to the drop amount.
			var drop = control * delta * frictionAmount;

			// scale the velocity
			float newspeed = speed - drop;
			if ( newspeed < 0 ) newspeed = 0;
			if ( newspeed == speed ) return;

			newspeed /= speed;
			Velocity *= newspeed;
		}
	}
}
