public sealed class HoleGoal : Component, Component.ITriggerListener
{
	/// <summary>
	/// Which hole is this a goal for?
	/// </summary>
	public Hole Hole => GetComponentInParent<Hole>();

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
	public float CupTime { get; set; } = 0.25f;

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		var whirl = ball.GetComponent<GoalWhirlpoolEffect>();
		whirl.StartWhirlpoolEffect( this );

		balls.Add( ball, 0 );
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		var whirl = ball.GetComponent<GoalWhirlpoolEffect>();
		whirl.IsSinking = false;

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
		if ( IsProxy )
			return;

		foreach ( var kv in balls )
		{
			// Have to be cupped for over half a second 
			if ( kv.Value > CupTime )
			{
				kv.Key.IsCupped = true;

				using ( Rpc.FilterInclude( kv.Key.Network.Owner ) )
				{
					BroadcastGoal( kv.Key );
				}

				balls.Remove( kv.Key );
			}
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	private void BroadcastGoal( Ball ball )
	{
		IGameEvent.Post( x => x.OnGoal( ball, this ) );
	}
}
