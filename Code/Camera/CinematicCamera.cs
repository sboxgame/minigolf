namespace Facepunch.Minigolf;

public partial class CinematicCamera : BaseCamera
{
	/// <summary>
	/// How fast do we orbit around a target?
	/// </summary>
	[Property]
	public float OrbitSpeed { get; set; } = 0.25f;

	/// <summary>
	/// Distance of orbit
	/// </summary>
	[Property]
	public float OrbitDistance { get; set; } = 300.0f;

	/// <summary>
	/// Height above target when orbiting
	/// </summary>
	[Property]
	public float Height { get; set; } = 90.0f;

	TimeUntil TimeUntilNextTarget { get; set; }
	public GameObject Target { get; set; }

	private GameObject FindRandomTarget()
	{
		return Scene.GetAllComponents<HoleGoal>()
			.FirstOrDefault( x => x.GameObject != Target )
			.GameObject;
	}

	private void UpdateWaiting()
	{
		if ( TimeUntilNextTarget )
		{
			Target = FindRandomTarget();
			TimeUntilNextTarget = 5f;
		}

		if ( Target.IsValid() )
		{
			OrbitAroundTarget();
		}
	}

	private float angle;
	private void OrbitAroundTarget()
	{
		angle += OrbitSpeed * Time.Delta;

		float x = MathF.Cos( angle ) * OrbitDistance;
		float z = MathF.Sin( angle ) * OrbitDistance;

		Camera.WorldPosition = new Vector3( Target.WorldPosition.x + x, Target.WorldPosition.y + z, Target.WorldPosition.z + Height );

		var normal = (Camera.WorldPosition - Target.WorldPosition).Normal;

		Camera.WorldRotation = Rotation.LookAt( -normal );
	}

	private void UpdateNewHole()
	{
		Target = GameManager.Instance.CurrentHole.GameObject;

		if ( Target.IsValid() )
		{
			OrbitAroundTarget();
		}
	}

	public override void OnCameraUpdate()
	{
		UpdateViewBlockers();

		var state = GameManager.Instance.State;

		if ( state is GameState.WaitingForPlayers )
		{
			UpdateWaiting();
		}
		if ( state is GameState.NewHole )
		{
			UpdateNewHole();
		}
	}
}
