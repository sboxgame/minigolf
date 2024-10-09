namespace Facepunch.Minigolf;

public partial class GameManager
{
	[Sync]
	public Hole CurrentHole { get; set; }

	/// <summary>
	/// What's the next hole?
	/// </summary>
	/// <returns></returns>
	private Hole GetNextHole()
	{
		var currentNumber = CurrentHole.Number;

		return Scene.GetAllComponents<Hole>()
			.FirstOrDefault( x => x.Number == currentNumber + 1 );
	}

	public async void EndHole()
	{
		var nextHole = GetNextHole();

		if ( !nextHole.IsValid() )
		{
			EndGame();
			return;
		}
		
		// TODO: Announce state

		await GameTask.DelaySeconds( 5.0f );

		CurrentHole = nextHole;

		var position = CurrentHole.WorldPosition + Vector3.Up * 16f;
		foreach ( var ball in Scene.GetAllComponents<Ball>() )
		{
			ball.IsCupped = false;
			ball.ResetPosition( position, Angles.Zero );
		}
	}

	public void EndGame()
	{
		// TODO: End the game
	}

	/// <summary>
	/// Called when the ball changes play mode
	/// </summary>
	/// <param name="ball"></param>
	/// <param name="inPlay"></param>
	void IGameEvent.BallInPlay( Ball ball, bool inPlay )
	{
		Log.Info( $"{ball} changed to {inPlay}" );
	}

	/// <summary>
	/// Called when the ball gets hit
	/// </summary>
	/// <param name="ball"></param>
	void IGameEvent.BallStroke( Ball ball )
	{
		Log.Info( $"{ball} struck" );
	}

	void IGameEvent.OnGoal( Ball ball, HoleGoal goal )
	{
		Log.Info( $"{ball} hit {goal}" );

		ball.IsCupped = true;
		Sound.Play( "minigolf.sink_into_cup", goal.WorldPosition );

		// TODO: All balls need to be cupped to end the hole
		EndHole();
	}

}
