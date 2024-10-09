using System.Diagnostics;

/// <summary>
/// Controls the camera
/// </summary>
public sealed class CameraController : Component
{
	/// <summary>
	/// Are we spectating right now?
	/// </summary>
	public bool IsSpectating { get; set; } = false;

	/// <summary>
	/// The ball we're following
	/// </summary>
	[Property]
	public Ball Ball { get; set; }

	/// <summary>
	/// The camera component
	/// </summary>
	[Property]
	public CameraComponent Camera { get; set; }

	/// <summary>
	/// How close can we zoom in
	/// </summary>
	[Property]
	public float MinDistance { get; set; } = 100.0f;

	/// <summary>
	/// How far away we can zoom out
	/// </summary>
	[Property]
	public float MaxDistance { get; set; } = 300.0f;

	/// <summary>
	/// The distance step each time we use the mouse wheel
	/// </summary>
	[Property]
	public float DistanceStep { get; set; } = 10.0f;

	/// <summary>
	/// How far up above the ball is the camera by default?
	/// </summary>
	[Property]
	public float UpAmount { get; set; } = 24f;

	private Angles _targetAngles;
	private Rotation _targetRotation;
	private float _distance = 150.0f;
	private float _targetDistance = 150.0f;

	private readonly List<ViewBlocker> _viewBlockers = new();
	private void UpdateViewBlockers()
	{
		foreach ( var ent in _viewBlockers )
		{
			ent.BlockingView = false;
		}

		_viewBlockers.Clear();

		var traces = Scene.Trace.Ray( Camera.WorldPosition, Ball.Local.WorldPosition )
			.RunAll();

		if ( traces == null )
			return;

		foreach ( var tr in traces )
		{
			if ( tr.Component.GetComponent<ViewBlocker>() is { } blocker )
			{
				blocker.BlockingView = true;
				_viewBlockers.Add( blocker );
			}
		}
	}

	protected override void OnUpdate()
	{
		_distance = Math.Clamp( _distance + -Input.MouseWheel.y * DistanceStep, MinDistance, MaxDistance );

		_targetAngles.yaw += Input.AnalogLook.yaw;
		_targetAngles = _targetAngles.Normal;

		if ( IsSpectating || !Input.Down( "Attack1" ) )
			_targetAngles.pitch += Input.AnalogLook.pitch;

		if ( IsSpectating || !Input.Down( "Attack1" ) )
			_targetAngles.pitch = _targetAngles.pitch.Clamp( 0, 89 );

		Camera.WorldPosition = Ball.WorldPosition + Vector3.Up * UpAmount;
		_targetRotation = Rotation.From( _targetAngles );

		Camera.WorldRotation = Rotation.Slerp( Camera.WorldRotation, _targetRotation, Time.Delta * 10.0f );
		_targetDistance = _targetDistance.LerpTo( _distance, Time.Delta * 5.0f );
		Camera.WorldPosition += Camera.WorldRotation.Backward * _targetDistance;
		Camera.FieldOfView = Preferences.FieldOfView;

		UpdateViewBlockers();
	}
}
