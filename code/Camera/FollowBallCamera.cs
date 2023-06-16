using Facepunch.Minigolf.Entities;
using Facepunch.Minigolf.Entities.Desktop;

namespace Facepunch.Minigolf;

public class FollowBallCamera : BaseCamera
{
	[Ball.ComponentDependency] private DesktopInputComponent DesktopInput { get; set; }
	private List<MiniProp> viewblockers = new();

	// should only need TargetRotation but I'm shit
	public Angles TargetAngles;
	Rotation TargetRotation;

	private float Distance = 150.0f;
	private float TargetDistance = 150.0f;

	public float MinDistance => 100.0f;
	public float MaxDistance => 300.0f;
	public float DistanceStep => 10.0f;

	public Ball Ball => Target is null ? Entity as Ball : Target;
	public Ball Target { get; set; }

	public override bool CanAddToEntity( Entity entity )
	{
		// Can only work on your balls
		return entity is Ball;
	}

	public override void Update()
	{
		if ( !Ball.IsValid() ) return;

		UpdateViewBlockers( Ball );

		Camera.Position = Ball.Position + Vector3.Up * (24 + (Ball.CollisionBounds.Center.z * Ball.Scale));
		TargetRotation = Rotation.From( TargetAngles );

		Camera.Rotation = Rotation.Slerp( Camera.Rotation, TargetRotation, RealTime.Delta * 10.0f );
		TargetDistance = TargetDistance.LerpTo( Distance, RealTime.Delta * 5.0f );
		Camera.Position += Camera.Rotation.Backward * TargetDistance;

		Camera.FieldOfView = 80.0f;

		var center = Ball.Position + Vector3.Up * 80;
		var distance = 150.0f * Ball.Scale;
		var targetPos = center + DesktopInput.ViewAngles.ToRotation().Forward * -distance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( Ball )
			.Radius( 8 )
			.Run();

		var endpos = tr.EndPosition;

		if ( tr.Entity is MiniProp ufp )
		{
			if ( ufp.NoCameraCollide )
				endpos = targetPos;
		}
	}

	public override void BuildInput()
	{
		// We take control of the camera when the ball is cupped.
		if ( Ball is null || Ball.Cupped )
			return;

		Distance = Math.Clamp( Distance + (-Input.MouseWheel * DistanceStep), MinDistance, MaxDistance );

		TargetAngles.yaw += Input.AnalogLook.yaw;

		if ( !Input.Down( InputActions.Attack1 ) )
			TargetAngles.pitch += Input.AnalogLook.pitch;

		TargetAngles = TargetAngles.Normal;

		if ( !Input.Down( InputActions.Attack1 ) )
			TargetAngles.pitch = TargetAngles.pitch.Clamp( 0, 89 );
	}

	private void UpdateViewBlockers( Ball pawn )
	{
		foreach ( var ent in viewblockers )
		{
			ent.BlockingView = false;
		}
		viewblockers.Clear();

		var traces = Trace.Sphere( 3f, Camera.Position, pawn.Position ).RunAll();

		if ( traces == null ) return;

		foreach ( var tr in traces )
		{
			if ( tr.Entity is not MiniProp prop ) continue;
			prop.BlockingView = true;
			viewblockers.Add( prop );
		}
	}
}
