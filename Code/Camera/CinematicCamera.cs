namespace Facepunch.Minigolf;

/// <summary>
/// A cinematic camera script which makes the camera orbit around objects.
/// </summary>
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

	/// <summary>
	/// How long until we switch target?
	/// </summary>
	TimeUntil TimeUntilNextTarget { get; set; }

	/// <summary>
	/// What's our current target?
	/// </summary>
	GameObject Target { get; set; }

	/// <summary>
	/// What hole are we on?
	/// </summary>
	Hole CurrentHole => GameManager.Instance.CurrentHole;

	/// <summary>
	/// Looks for a random target.
	/// </summary>
	/// <returns></returns>
	private GameObject FindRandomTarget<T>() where T : Component
	{
		return Scene.GetAllComponents<T>()
			.FirstOrDefault( x => x.GameObject != Target )
			.GameObject;
	}

	/// <summary>
	/// Special case that updates the target every 5 seconds.
	/// </summary>
	private void UpdateWaiting()
	{
		if ( TimeUntilNextTarget )
		{
			Target = FindRandomTarget<HoleGoal>();
			TimeUntilNextTarget = 5f;
		}

		if ( Target.IsValid() )
		{
			OrbitAroundTarget();
		}
	}

	// Cached angles.
	private float angle;

	/// <summary>
	/// Orbits around <see cref="Target"/> at a set speed and distance.
	/// </summary>
	private void OrbitAroundTarget()
	{
		angle += OrbitSpeed * Time.Delta;

		float x = MathF.Cos( angle ) * OrbitDistance;
		float z = MathF.Sin( angle ) * OrbitDistance;

		Camera.WorldPosition = Camera.WorldPosition.LerpTo( new Vector3( Target.WorldPosition.x + x, Target.WorldPosition.y + z, Target.WorldPosition.z + Height ), Time.Delta * 10f );

		var normal = (Camera.WorldPosition - Target.WorldPosition).Normal;

		Camera.WorldRotation = Rotation.LookAt( -normal );
	}

	/// <summary>
	/// Updates the camera to orbit around a set target
	/// </summary>
	/// <param name="target"></param>
	private void UpdateWithTarget( GameObject target )
	{
		Target = target;

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
			UpdateWithTarget( CurrentHole.Start.GameObject );
		}
		if ( state is GameState.HoleFinished )
		{
			UpdateWithTarget( CurrentHole.Goal.GameObject );
		}
	}
}
