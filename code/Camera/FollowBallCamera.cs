using Sandbox;
using System;

namespace Minigolf
{
	public class FollowBallCamera : ICamera
	{
		public Angles Angles;
		public Vector3 Position;

		private float Distance;
		private float distanceTarget;

		public float MinDistance => 100.0f;
		public float MaxDistance => 300.0f;
		public float DistanceStep => 10.0f;

		public Ball Ball;

		public FollowBallCamera()
		{
			Distance = 150;
			distanceTarget = Distance;
		}

		public override void Build( ref CameraSetup camSetup )
		{
			if ( !Ball.IsValid() ) return;

			var pos = Ball.Position + Vector3.Up * (24 + (Ball.CollisionBounds.Center.z * Ball.Scale));
			var rot = Rotation.From( Angles );

			distanceTarget = distanceTarget.LerpTo( Distance, Time.Delta * 5.0f );

			// TODO: Camera snapping like this is horrible, do a better trace to prevent the camera being inside objects instead.
			/*
			var tr = Trace.Ray( pos, pos + rot.Backward * Distance )
				.Ignore( Ball )
				.WorldOnly()
				.Radius( 8 )
				.Run();

			if ( tr.Hit )
			{
				distanceTarget = Math.Min( distanceTarget, tr.Distance );
			}
			*/

			pos += rot.Backward * distanceTarget;

			// ball.RenderAlpha = Math.Clamp( (distanceTarget - 25.0f) / 50.0f, 0.0f, 1.0f );

			// TODO: If the ball is cupped, take control and rotate around it cinematically
			// Zoom out the camera to max distance and rotate.

			camSetup.Position = pos;
			camSetup.Rotation = rot;
			camSetup.FieldOfView = 80;
		}

		public override void BuildInput( InputBuilder input )
		{
			// We take control of the camera when the ball is cupped.
			if ( Ball.Cupped )
				return;

			Distance = Math.Clamp( Distance + (-input.MouseWheel * DistanceStep), MinDistance, MaxDistance );

			Angles.yaw += input.AnalogLook.yaw;

			if ( !input.Down( InputButton.Attack1 ) )
				Angles.pitch += input.AnalogLook.pitch;

			Angles = Angles.Normal;

			if ( !input.Down( InputButton.Attack1 ) )
				Angles.pitch = Angles.pitch.Clamp( 0, 89 );
		}
	}
}
