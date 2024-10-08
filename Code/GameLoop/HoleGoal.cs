public sealed class HoleGoal : Component, Component.ITriggerListener
{
	/// <summary>
	/// Which hole is this a goal for?
	/// </summary>
	[Property] 
	public Hole Hole { get; set; }

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		IGameEvent.Post( x => x.OnGoal( ball, this ) );
	}
}
