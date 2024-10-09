namespace Facepunch.Minigolf;

public enum GameState
{
	WaitingForPlayers,
	InPlay,
	EndOfGame
}

public partial class GameManager
{
	[HostSync]
	public Hole CurrentHole { get; set; }

	[HostSync]
	public bool IsHoleEnding { get; set; }

	[HostSync]
	public GameState State { get; set; } = GameState.WaitingForPlayers;

	/// <summary>
	/// Ran on update, check for any uncupped balls - if there are any, we won't end the hole.
	/// 
	/// This is easier than responding to states, but we might want to change it to that at some point.
	/// </summary>
	private void CheckRoundState()
	{
		if ( IsHoleEnding ) return;

		var waitingCount = Scene.GetAllComponents<Ball>()
			.Where( x => !x.IsCupped )
			.Count();
		
		if ( waitingCount == 0 )
			EndHole();
	}

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

	/// <summary>
	/// Ends the current hole.
	/// </summary>
	public async void EndHole()
	{
		var nextHole = GetNextHole();

		if ( !nextHole.IsValid() )
		{
			EndGame();
			return;
		}

		IsHoleEnding = true;

		// TODO: Announce state
		await GameTask.DelaySeconds( 5.0f );

		CurrentHole = nextHole;
		IsHoleEnding = false;

		if ( !CurrentHole.IsValid() )
			return;

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
	}

}
