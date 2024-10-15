namespace Facepunch.Minigolf;

public sealed class StatsListener : Component, 
	IGameEvent, IPlayerEvent
{
	/// <summary>
	/// Called when we hit the ball
	/// </summary>
	/// <param name="ball"></param>
	void IGameEvent.BallStroke( Ball ball ) 
	{
		Stats.Increment( "total_strokes" );
		Stats.Increment( "total_strokes", 1, true );
	}

	void IGameEvent.OnGoal( Ball ball, HoleGoal goal )
	{
		// We only want to increment stats if we're the ball owner
		if ( ball.IsProxy )
			return;

		Stats.Increment( "goals" );
		Stats.Increment( "goals", 1, true );
	}
}
