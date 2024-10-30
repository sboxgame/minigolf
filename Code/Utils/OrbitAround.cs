public sealed class OrbitAround : Component
{
	[Property]
	public Vector3 OrbitCenter { get; set; }

	[Property]
	public float OrbitSpeed { get; set; } = 0.1f;
	
	[Property]
	public float OrbitRadius { get; set; } = 6f;

	[Property]
	private float CurrentAngle { get; set; } = 0f;

	protected override void OnUpdate()
	{
		CurrentAngle += OrbitSpeed * Time.Delta;
		CurrentAngle %= 360f;

		float angleInRadians = CurrentAngle.RadianToDegree();

		float x = OrbitCenter.x + MathF.Cos( angleInRadians ) * OrbitRadius;
		float y = OrbitCenter.y + MathF.Sin( angleInRadians ) * OrbitRadius;

		LocalPosition = new Vector3( x, y, 0 );
	}
}
