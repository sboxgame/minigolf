using Sandbox;
using System;

using Facepunch.Minigolf.Entities;

namespace Facepunch.Minigolf;

public class FollowBallCamera : ICamera
{
	public Angles Angles { get { return Rot.Angles(); } set { TargetAngles = value; } }
	public float FieldOfView => 80.0f;

	Vector3 Pos;
	Rotation Rot;

	Angles TargetAngles;
	Rotation TargetRot;

	private float Distance;
	private float TargetDistance;

	public float MinDistance => 100.0f;
	public float MaxDistance => 300.0f;
	public float DistanceStep => 10.0f;

	public Ball Ball;

	public FollowBallCamera()
	{
		Distance = 150;
		TargetDistance = Distance;
	}

	public override void Build( ref CameraSetup camSetup )
	{
		if ( !Ball.IsValid() ) return;

		Pos = Ball.Position + Vector3.Up * (24 + (Ball.CollisionBounds.Center.z * Ball.Scale));
		TargetRot = Rotation.From( TargetAngles );

		Rot = Rotation.Slerp( Rot, TargetRot, RealTime.Delta * 10.0f );
		TargetDistance = TargetDistance.LerpTo( Distance, RealTime.Delta * 5.0f );
		Pos += Rot.Backward * TargetDistance;

		camSetup.Position = Pos;
		camSetup.Rotation = Rot;
	}

	public override void BuildInput( InputBuilder input )
	{
		// We take control of the camera when the ball is cupped.
		if ( Ball.Cupped )
			return;

		Distance = Math.Clamp( Distance + (-input.MouseWheel * DistanceStep), MinDistance, MaxDistance );

		TargetAngles.yaw += input.AnalogLook.yaw;

		if ( !input.Down( InputButton.Attack1 ) )
			TargetAngles.pitch += input.AnalogLook.pitch;

		TargetAngles = TargetAngles.Normal;

		if ( !input.Down( InputButton.Attack1 ) )
			TargetAngles.pitch = TargetAngles.pitch.Clamp( 0, 89 );
	}
}