public sealed class HoleGoal : Component, Component.ITriggerListener
{
	/// <summary>
	/// Which hole is this a goal for?
	/// </summary>
	[Property] 
	public Hole Hole { get; set; }

	/// <summary>
	/// Particle for hole number
	/// </summary>
	[Property]
	public LegacyParticleSystem Particles { get; set; }

	/// <summary>
	/// Maintain a list of balls, so we can confirm that they've been cupped after <see cref="CupTime"/>
	/// </summary>
	private Dictionary<Ball, RealTimeSince> balls { get; set; } = new();

	/// <summary>
	/// How long until we confirm a cupped ball?
	/// </summary>
	[Property]
	public float CupTime { get; set; } = 0.5f;

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		balls.Add( ball, 0 );
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		balls.Remove( ball );
	}

	protected override void OnStart()
	{
		var firstNum = Hole.Number % 10;
		var secondNum = ( Hole.Number / 10 ) % 10;
		Particles.SceneObject.SetControlPoint( 21, new Vector3( 0, secondNum, firstNum ) );
	}

	protected override void OnUpdate()
	{
		foreach ( var kv in balls )
		{
			// Have to be cupped for over half a second 
			if ( kv.Value > CupTime )
			{
				IGameEvent.Post( x => x.OnGoal( kv.Key, this ) );
				balls.Remove( kv.Key );
			}
		}
	}
}
