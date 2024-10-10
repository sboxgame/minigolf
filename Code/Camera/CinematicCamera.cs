namespace Facepunch.Minigolf;

public partial class CinematicCamera : BaseCamera
{
	TimeUntil TimeUntilNextTarget { get; set; }

	public GameObject Target { get; set; }

	private GameObject FindRandomTarget()
	{
		return Scene.GetAllComponents<HoleGoal>()
			.FirstOrDefault( x => x.GameObject != Target )
			.GameObject;
	}

	public override void OnCameraUpdate()
	{
		if ( TimeUntilNextTarget )
		{
			Target = FindRandomTarget();
			TimeUntilNextTarget = 5f;
		}

		if ( Target.IsValid() )
		{
			Camera.WorldPosition = Target.WorldPosition + Vector3.Backward * 256f + Vector3.Up * 64f;
			Camera.WorldRotation = Rotation.LookAt( Target.WorldPosition, Vector3.Up );
		}
	}
}
