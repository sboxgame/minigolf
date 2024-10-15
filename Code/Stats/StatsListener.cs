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

		// Store what our par was for this hole for this map, and we can do stuff with it.
		Stats.Increment( $"par-{goal.Hole.Number}", ball.GetCurrentPar() );

		// Hole in ones!
		if ( ball.GetCurrentPar() == 1 )
		{
			Stats.Increment( "hole-in-ones", 1, false );
			Stats.Increment( "hole-in-ones" );
			Stats.Increment( $"hole-in-ones-{goal.Hole.Number}" );
		}
	}
}
