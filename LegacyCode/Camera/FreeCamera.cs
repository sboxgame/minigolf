namespace Facepunch.Minigolf;

public class FreeCamera : BaseCamera
{
	public static float RemainingTime { get; private set; } = 30f;
	public bool Stale { get; set; } = true;

	private Angles _lookAngles;
	private Vector3 _moveInput;
	private Vector3 _targetPos;
	private Rotation _targetRot;
	private float _moveSpeed;

	public FreeCamera()
	{
		Reset();
	}

	public override void Update()
	{
		if ( Stale )
			Reset();

		Camera.FirstPersonViewer = null;
		RemainingTime -= RealTime.Delta;
		FreeMove();
	}

	public override void BuildInput()
	{
		_moveInput = Input.AnalogMove;

		_moveSpeed = 1;
		if ( Input.Down( InputActions.Run ) ) _moveSpeed = 5;

		_lookAngles += Input.AnalogLook;
		_lookAngles.roll = 0;

		Input.Clear( InputActions.Attack1 );
		Input.StopProcessing = true;
	}

	private void Reset()
	{
		_targetPos = Camera.Position;
		_targetRot = Camera.Rotation;

		Camera.Position = _targetPos;
		Camera.Rotation = _targetRot;

		_lookAngles = Camera.Rotation.Angles();

		Stale = false;
	}

	private void FreeMove()
	{
		var mv = _moveInput.Normal * 300 * RealTime.Delta * Camera.Rotation * _moveSpeed;

		_targetRot = Rotation.From( _lookAngles );
		_targetPos += mv;

		Camera.Position = Vector3.Lerp( Camera.Position, _targetPos, 10 * RealTime.Delta );
		Camera.Rotation = Rotation.Slerp( Camera.Rotation, _targetRot, 10 * RealTime.Delta );
	}
}
