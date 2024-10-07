using Facepunch.Minigolf.Entities;

namespace Facepunch.Minigolf;

public class FollowBallCamera : BaseCamera
{
	public Ball Target { get; set; }
	public Angles TargetAngles;

	private float MinDistance => 100.0f;
	private float MaxDistance => 300.0f;
	private float DistanceStep => 10.0f;
	private Ball Ball => Target ?? Entity as Ball;
	private bool IsSpectating => Target.IsValid() && !Target.IsLocalPawn;
	private Rotation _targetRotation;
	private float _distance = 150.0f;
	private float _targetDistance = 150.0f;
	private readonly List<MiniProp> _viewBlockers = new();

	public override bool CanAddToEntity( Entity entity )
	{
		// Can only work on your balls
		return entity is Ball;
	}

	public override void Update()
	{
		if ( !Ball.IsValid() ) 
			return;

		UpdateViewBlockers( Ball );

		Camera.Position = Ball.Position + Vector3.Up * (24 + (Ball.CollisionBounds.Center.z * Ball.Scale));
		_targetRotation = Rotation.From( TargetAngles );

		Camera.Rotation = Rotation.Slerp( Camera.Rotation, _targetRotation, RealTime.Delta * 10.0f );
		_targetDistance = _targetDistance.LerpTo( _distance, RealTime.Delta * 5.0f );
		Camera.Position += Camera.Rotation.Backward * _targetDistance;
		Camera.FieldOfView = 80.0f;
	}

	public override void BuildInput()
	{
		_distance = Math.Clamp( _distance + -Input.MouseWheel * DistanceStep, MinDistance, MaxDistance );

		TargetAngles.yaw += Input.AnalogLook.yaw;
		TargetAngles = TargetAngles.Normal;

		if ( IsSpectating || !Input.Down( InputActions.Attack1 ) )
			TargetAngles.pitch += Input.AnalogLook.pitch;

		if ( IsSpectating || !Input.Down( InputActions.Attack1 ) )
			TargetAngles.pitch = TargetAngles.pitch.Clamp( 0, 89 );

		if ( IsSpectating && Input.Pressed( InputActions.Attack1 ) )
			SwapSpectatedBall( false );

		if ( IsSpectating && Input.Pressed( InputActions.Attack2 ) )
			SwapSpectatedBall( true );
	}

	private void SwapSpectatedBall( bool advance )
	{
		var balls = Entity.All.OfType<Ball>().ToList();
		var spectatingIndex = balls.IndexOf( Ball );
		var newSpectatedBall = balls.ElementAtOrDefault( advance ? spectatingIndex += 1 : spectatingIndex -= 1 );
		Target = newSpectatedBall ?? (advance ? balls.FirstOrDefault() : balls.LastOrDefault());
	}

	private void UpdateViewBlockers( Ball pawn )
	{
		foreach ( var ent in _viewBlockers )
		{
			ent.BlockingView = false;
		}

		_viewBlockers.Clear();

		var traces = Trace.Sphere( 3f, Camera.Position, pawn.Position ).RunAll();

		if ( traces == null ) 
			return;

		foreach ( var tr in traces )
		{
			if ( tr.Entity is not MiniProp prop ) continue;
			prop.BlockingView = true;
			_viewBlockers.Add( prop );
		}
	}
}
