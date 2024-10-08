public sealed class BallController : Component
{
	[RequireComponent] public Ball Ball { get; set; }

	[Sync]
	public float ShotPower { get; set; } = 0f;

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( Input.Down( "Attack1" ) )
		{
			float delta = Input.AnalogLook.pitch * RealTime.Delta;
			ShotPower = Math.Clamp( ShotPower - delta, 0, 1 );
		}

		if ( ShotPower > 0.0f && Input.Released( "Attack1" ) )
		{
			Ball.Stroke( Scene.Camera.WorldRotation.Yaw(), ShotPower );
			ShotPower = 0f;
		}
	}
}
