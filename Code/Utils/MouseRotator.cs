public sealed class MouseRotator : Component
{
	[Property]
	public float DeltaSpeed { get; set; } = 0.025f;

	[Property]
	public float LerpSpeed { get; set; } = 10f;

	private float AddedYaw;

	protected override void OnUpdate()
	{
		if ( Input.Down( "Attack1" ) )
		{
			var delta = Mouse.Delta;
			AddedYaw += delta.x * DeltaSpeed;
		}

		WorldRotation *= Rotation.From( 0, AddedYaw, 0 );
		AddedYaw = AddedYaw.LerpTo( 0, Time.Delta * LerpSpeed );
	}
}
