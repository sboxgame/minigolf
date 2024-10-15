namespace Facepunch.Minigolf;

/// <summary>
/// Place this in a scene, and we'll listen to events that give stats to players for certain gameplay events.
/// </summary>
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
		Stats.Increment( "total_strokes", 1, false );
	}

	void IGameEvent.OnGoal( Ball ball, HoleGoal goal )
	{
		// We only want to increment stats if we're the ball owner
		if ( ball.IsProxy )
			return;

		Stats.Increment( "goals" );
		Stats.Increment( "goals", 1, false );
	}
}
