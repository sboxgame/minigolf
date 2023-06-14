namespace Facepunch.Minigolf;

public class HoleEndCamera : BaseCamera
{
	Vector3 TargetPosition;
	Rotation TargetRotation;

	public Vector3 HolePosition { get; set; }

	float LerpSpeed => 4.0f;
	float DistanceAwayFromHole => 250.0f;

	float Rot = 0.0f;

	public override void Update()
	{
		Rot += RealTime.Delta * 10.0f;

		Rotation rot = Rotation.FromYaw( Rot );

		Vector3 dir = (Vector3.Up * 0.35f) + (Vector3.Forward * rot);
		dir = dir.Normal;

		TargetPosition = HolePosition + Vector3.Up * 50.0f + dir * DistanceAwayFromHole;
		TargetRotation = Rotation.From( (-dir).EulerAngles );

		// Slerp slerp
		Camera.Position = Camera.Position.LerpTo( TargetPosition, RealTime.Delta * LerpSpeed );
		Camera.Rotation = Rotation.Slerp( Camera.Rotation, TargetRotation, RealTime.Delta * LerpSpeed );
	}

	public override void BuildInput() { }
}
