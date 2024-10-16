using Sandbox;

public sealed class RotateOverTime : Component
{
	[Property] public float Speed { get; set; } = 5f;
	[Property] public Vector3 Direction { get; set; } = new( 1, 0, 0 );
	 
	protected override void OnUpdate()
	{
		var spd = Speed * Time.Delta;
		WorldRotation *= Rotation.From( Direction.x * spd, Direction.y * spd, Direction.z * spd );
	}
}
