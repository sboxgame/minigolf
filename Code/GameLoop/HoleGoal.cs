public sealed class HoleGoal : Component, Component.ITriggerListener
{
	/// <summary>
	/// Which hole is this a goal for?
	/// </summary>
	[Property] 
	public Hole Hole { get; set; }

	[Property]
	public LegacyParticleSystem Particles { get; set; }

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		IGameEvent.Post( x => x.OnGoal( ball, this ) );
	}

	protected override void OnStart()
	{
		var firstNum = Hole.Number % 10;
		var secondNum = ( Hole.Number / 10 ) % 10;
		Particles.SceneObject.SetControlPoint( 21, new Vector3( 0, firstNum, secondNum ) );
	}
}
