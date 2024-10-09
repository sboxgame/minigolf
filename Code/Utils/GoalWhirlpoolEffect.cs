/// <summary>
/// A quick and nasty class that produces a whirlpooling effect on the ball as it goes down into a hole's goal.
/// </summary>
public sealed class GoalWhirlpoolEffect : Component
{
	/// <summary>
	/// The ball's rigidbody, since it's placed on the ball
	/// </summary>
	[RequireComponent] 
	public Rigidbody Rigidbody { get; set; }

	/// <summary>
	/// Is teh ball sinking?
	/// </summary>
	[Property] public bool IsSinking { get; set; }

	/// <summary>
	/// The target goal hole
	/// </summary>
	[Property] public HoleGoal HoleGoal { get; set; }

	/// <summary>
	/// How effective is the radial force
	/// </summary>
	[Property]
	public float WhirlpoolStrength { get; set; } = 1000f;

	/// <summary>
	/// How much force to pull the ball into the center of the hole
	/// </summary>
	[Property]
	public float PullStrength { get; set; } = 200f;

	/// <summary>
	/// Distance at which the ball "sinks" into the hole
	/// </summary>
	[Property]
	public float SinkThreshold { get; set; } = 1.5f;    

	/// <summary>
	/// How much the ball slows down when nearing the center
	/// </summary>
	[Property]
	public float SlowFactor { get; set; } = 0.95f; 

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( IsSinking )
		{
			// Calculate direction from the ball to the hole
			var directionToHole = ( HoleGoal.WorldPosition - WorldPosition ).Normal;
			var distanceToHole = directionToHole.Length;

			// If the ball is close enough to the center, "sink" it, by letting go
			if ( distanceToHole < SinkThreshold )
				return;

			var tangentDir = Vector3.Cross( directionToHole, Vector3.Up ).Normal;
			Rigidbody.Velocity += tangentDir * WhirlpoolStrength * Time.Delta;
			Rigidbody.ApplyForce( directionToHole * PullStrength * Time.Delta );

			Rigidbody.Velocity *= SlowFactor;
		}
	}

	/// <summary>
	/// Called the ball reaches the hole to start the sinking effect
	/// </summary>
	/// <param name="hole"></param>
	public void StartWhirlpoolEffect( HoleGoal hole )
	{
		if ( IsProxy ) return;

		HoleGoal = hole;
		IsSinking = true;
	}
}
