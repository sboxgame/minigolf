using Sandbox;
using System;

namespace Minigolf
{
	public partial class Ball
	{
		[ServerVar( "minigolf_test_clipvelocity" )]
		public static bool ClipVelocityTest { get; set; } = false;

		[ServerVar( "minigolf_test_nophysoverride" )]
		public static bool NoPhysicsOverride { get; set; } = false;

		static float PowerMultiplier => 2500.0f;

		// Damping values
		static float DefaultLinearDamping => 0.1f;
		static float DefaultAngularDamping => 4.00f;
		static float UpHillLinearDamping => 0.015f;
		static float UpHillAngularDamping => 2.00f;
		static float DownHillLinearDamping => 0.0f;
		static float DownHillAngularDamping => 1.0f;

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( IsServer )
				Move();
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			DebugOverlay.Text( Position, $"Velocity: {Velocity}" );
			DebugOverlay.Box( Position, -3.0f, 3.0f, Color.Green, false );
		}

		public virtual void Move()
		{
			Velocity += Vector3.Down * 800 * Time.Delta;

			MoveHelper mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( 6.0f ).Ignore( this );
			mover.MaxStandableAngle = 45.0f;
			mover.GroundBounce = 0.25f;
			mover.WallBounce = 0.5f;

			mover.TryMove( Time.Delta );
			mover.ApplyFriction( mover.Velocity.Length < 1.0f ? 5.0f : 0.75f, Time.Delta );

			Position = mover.Position;
			Velocity = mover.Velocity;

			if ( mover.HitWall && mover.WallEntity is Wall )
			{
				ImpactEffects( mover.Position, mover.Velocity.Length );
			}
		}

		[ClientRpc]
		void ImpactEffects( Vector3 pos, float speed )
		{
			// Collision sound happens at this point, not entity
			var soundName = $"minigolf.ball_impact_on_concrete{ Rand.Int( 1, 4 ) }";
			var sound = Sound.FromWorld( soundName, pos );
			sound.SetVolume( 0.2f + Math.Clamp( speed / 1250.0f, 0.0f, 0.8f ) );
			sound.SetPitch( 0.5f + Math.Clamp( speed / 1250.0f, 0.0f, 0.5f ) );

			var particle = Particles.Create( "particles/gameplay/ball_hit/ball_hit.vpcf", pos );
			particle.SetPosition( 0, pos );
			particle.Destroy( false );
		}

		[Event.Tick.Server]
		protected void CheckInPlay()
		{
			InPlay = false;
			return;

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

		[Event.Physics.PreStep]
		protected void AdjustPhysics()
		{
			return;

			// BUG? Sometimes this event will run with an invalid physicsbody whilst being deleted.
			if ( !IsValid || !PhysicsBody.IsValid() )
				return;

			if ( Debug )
			{
				DebugOverlay.ScreenText( 0, $"Mass:             {PhysicsBody.Mass:F2}" );
				DebugOverlay.ScreenText( 1, $"Velocity:         {PhysicsBody.Velocity.Length:F2}/s" );
				DebugOverlay.ScreenText( 2, $"Angular Velocity: {PhysicsBody.AngularVelocity.Length:F2}" );

				DebugOverlay.ScreenText( 5, $"Linear Drag:     {PhysicsBody.LinearDrag:F2}" );
				DebugOverlay.ScreenText( 6, $"Linear Damping:  {PhysicsBody.LinearDamping:F2}" );
				DebugOverlay.ScreenText( 7, $"Angular Drag:    {PhysicsBody.AngularDrag:F2}" );
				DebugOverlay.ScreenText( 8, $"Angular Damping: {PhysicsBody.AngularDamping:F2}" );
			}

			// If the ball is in the hole, do nothing
			if ( Cupped )
				return;

			if ( Velocity.Length > 16.0f )
				Direction = Velocity.Normal.WithZ( 0 ).EulerAngles;

			if ( ClipVelocityTest )
				ClipVelocity();

			AdjustDamping();
		}

		protected void ClipVelocity()
		{
			var trace = Trace.Ray( Position, Position + Vector3.Down * CollisionBounds.Size.z * 1.15f );
			trace.HitLayer( CollisionLayer.Solid );
			trace.WithoutTags( "golf_ball" );
			var tr = trace.Run();

			if ( tr.Hit )
			{
				var backoff = PhysicsBody.Velocity.Dot( tr.Normal );
				var o = PhysicsBody.Velocity - ( tr.Normal * backoff );

				var adjust = o.Dot( tr.Normal );
				if ( adjust >= 1.0f )
				{
					PhysicsBody.Velocity = o;
					return;
				}

				adjust = MathF.Min( adjust, -1.0f );
				o -= tr.Normal * adjust;

				PhysicsBody.Velocity = o;
			}
		}

		protected void AdjustAirDamping()
		{
			// Just do the default values
			PhysicsBody.LinearDamping = DefaultLinearDamping;
			PhysicsBody.AngularDamping = DefaultAngularDamping;
		}

		/// <summary>
		/// We adjust the ball's linear / angular damping based on the surface.
		/// </summary>
		protected void AdjustDamping()
		{
			var downTrace = Trace.Ray( Position, Position + Vector3.Down * CollisionBounds.Size.z );
			downTrace.HitLayer( CollisionLayer.Solid );
			downTrace.WithoutTags( "golf_ball" );
			var downTraceResult = downTrace.Run();

			if ( Debug )
			{
				DebugOverlay.Line( downTraceResult.StartPos, downTraceResult.EndPos, 0, false );
				DebugOverlay.ScreenText( 10, $"On Ground:          {downTraceResult.Hit}" );
				DebugOverlay.ScreenText( 11, $"Ground Surface:     {downTraceResult.Surface.Name}" );
				DebugOverlay.ScreenText( 12, $"Surface Friction:   {downTraceResult.Surface.Friction}" );
				DebugOverlay.ScreenText( 13, $"Surface Elasticity: {downTraceResult.Surface.Elasticity}" );
				DebugOverlay.ScreenText( 14, $"Surface Dampening:  {downTraceResult.Surface.Dampening}" );
			}


			// If there is nothing below us we're in the air, handle that specifically.
			if ( !downTraceResult.Hit )
			{
				AdjustAirDamping();
				return;
			}

			// Give ourselves a shit load of damping at a low velocity, brings the ball to a faster stop in a more natural way.
			if ( InPlay && Velocity.Length < 10.0f )
			{
				PhysicsBody.LinearDamping = DefaultLinearDamping * 10.0f;
				PhysicsBody.AngularDamping = DefaultAngularDamping * 10.0f;

				return;
			}

			if ( NoPhysicsOverride )
			{
				PhysicsBody.LinearDamping = DefaultLinearDamping;
				PhysicsBody.AngularDamping = DefaultAngularDamping;
				return;
			}
			
			// See if we're on a flat surface by checking the dot product of the surface normal.
			// TODO: This is actually shit, what if we have these on a hill?
			if ( downTraceResult.Normal.Dot( Vector3.Up ).AlmostEqual( 1, 0.001f ) )
			{
				switch ( downTraceResult.Surface.Name )
				{
					case "minigolf.sand":
						PhysicsBody.LinearDamping = 2.5f;
						PhysicsBody.AngularDamping = 2.5f;
						break;
					case "minigolf.ice":
						PhysicsBody.LinearDamping = 0.25f;
						PhysicsBody.AngularDamping = 0.00f;
						break;
					default:
						PhysicsBody.LinearDamping = DefaultLinearDamping;
						PhysicsBody.AngularDamping = DefaultAngularDamping;
						break;
				}

				return;
			}

			// We must be on a hill, we can detect if it's up hill or down hill by doing a forward trace
			// TODO: This direction is wrong, we should be using the hill normal dot with 
			var trace = Trace.Ray( Position, Position + PhysicsBody.Velocity.WithZ( 0 ).Normal * CollisionBounds.Size.y * 2 );
			trace.HitLayer( CollisionLayer.Solid );
			trace.WithoutTags( "golf_ball" );
			var traceResult = trace.Run();

			if ( Debug )
			{
				DebugOverlay.Line( traceResult.StartPos, traceResult.EndPos, 0, false );
			}

			if ( traceResult.Hit )
			{
				PhysicsBody.LinearDamping = UpHillLinearDamping;
				PhysicsBody.AngularDamping = UpHillAngularDamping;
				return;
			}

			PhysicsBody.LinearDamping = DownHillLinearDamping;
			PhysicsBody.AngularDamping = DownHillAngularDamping;
		}

		/// <summary>
		/// Do our own physics collisions, we create a fun bouncing effect this way and handle collision sounds.
		/// </summary>
		/// <param name="eventData"></param>
		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			// We only want to override collision with sides, those should be func_brush or similar.
			if ( eventData.Entity.IsWorld )
			{
				// PhysicsBody.Velocity = eventData.PreVelocity;

				if ( Debug )
				{
					DebugOverlay.Sphere( eventData.Pos, 3, Color.White, false, 10.0f );
					DebugOverlay.Line( eventData.Pos, eventData.Pos + eventData.Normal * 8.0f, Color.Red, 10.0f, false );
				}

				return;
			}

			// Let the engine handle the low velocity collisions
			// if ( eventData.PreVelocity.Length < 10 )
			// 	return;

			// Hard code collisions with the wall for now.
			if ( eventData.Entity is Wall wall )
			{
				if ( !wall.Reflect )
				{
					// Most likely a curve, reset z to avoid weirdness
					PhysicsBody.Velocity = PhysicsBody.Velocity.WithZ( 0 );
					return;
				}

				ReflectBall( eventData, wall.ReflectMultiplier );
				return;
			}
		}

		protected void ReflectBall( CollisionEventData eventData, float multiplier )
		{
			var reflect = Vector3.Reflect( eventData.PreVelocity.Normal, eventData.Normal.Normal ).Normal;

			var normalDot = eventData.PreVelocity.Normal.Dot( eventData.Normal );

			// Don't do any reflection if we hit it at such an angle
			if ( normalDot <= 0.10 )
				return;

			// Collision sound happens at this point, not entity
			var soundName = $"minigolf.ball_impact_on_concrete{ Rand.Int( 1, 4 ) }";
			var sound = Sound.FromWorld( soundName, eventData.Pos );
			sound.SetVolume( 0.2f + Math.Clamp( eventData.Speed / 1250.0f, 0.0f, 0.8f ) );
			sound.SetPitch( 0.5f + Math.Clamp( eventData.Speed / 1250.0f, 0.0f, 0.5f ) );

			var particle = Particles.Create( "particles/gameplay/ball_hit/ball_hit.vpcf", eventData.Pos );
			particle.SetPosition( 0, eventData.Pos );
			particle.Destroy( false );

			var newSpeed = Math.Max( eventData.PreVelocity.Length, eventData.Speed );
			newSpeed *= multiplier;

			// Adjust the speed depending on the hit normal, slight hit = more speed
			newSpeed *= (1 - normalDot / 2);

			var newVelocity = reflect * newSpeed;

			// TODO: not a fan of this, should determine by the dot normal
			newVelocity.z = 0;

			PhysicsBody.Velocity = newVelocity;
			PhysicsBody.AngularVelocity = Vector3.Zero;

			if ( Debug )
			{
				DebugOverlay.Text( eventData.Pos, $"V {eventData.PreVelocity.Length} -> {newSpeed}", 5f );
				DebugOverlay.Text( eventData.Pos + Vector3.Up * 8, $"N. {normalDot}", 5f );
				DebugOverlay.Line( eventData.Pos, eventData.Pos - (eventData.PreVelocity.Normal * 64.0f), 5f );
				DebugOverlay.Line( eventData.Pos, eventData.Pos + (reflect * 64.0f), 5f );
			}
		}
	}
}
