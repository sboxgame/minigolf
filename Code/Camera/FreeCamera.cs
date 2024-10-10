using Facepunch.Minigolf;

/// <summary>
/// Controls the camera
/// </summary>
public sealed class FreeCamera : BaseCamera
{
	Angles freeCamAngles;

	public override void OnCameraActive()
	{
		freeCamAngles = Camera.WorldRotation.Angles();
	}

	public override void Tick()
	{
		Vector3 movement = Input.AnalogMove;
		float speed = 350.0f;

		freeCamAngles += Input.AnalogLook * 0.5f;
		freeCamAngles = freeCamAngles.WithPitch( freeCamAngles.pitch.Clamp( -89.0f, 89.0f ) );

		movement += Input.Down( "Jump" ) ? Vector3.Up : Vector3.Zero;
		movement -= Input.Down( "Duck" ) ? Vector3.Up : Vector3.Zero;

		if ( Input.Down( "run" ) )
		{
			speed = 750.0f;
		}

		Camera.WorldPosition += freeCamAngles.ToRotation() * movement * speed * Time.Delta;
		Camera.WorldRotation = freeCamAngles.ToRotation();

		UpdateViewBlockers();
	}
}
