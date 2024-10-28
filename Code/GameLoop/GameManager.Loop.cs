using Facepunch.Minigolf.UI;

namespace Facepunch.Minigolf;

public enum GameState
{
	WaitingForPlayers,
	InPlay,
	HoleFinished,
	NewHole,
	EndOfGame
}

public partial class GameManager
{
	/// <summary>
	/// What's our menu scene file?
	/// </summary>
	[Property]
	public SceneFile MenuSceneFile { get; set; }

	/// <summary>
	/// What's the duration of each hole until we skip to the next hole
	/// </summary>
	[ConVar( "minigolf_hole_length" )]
	public static int HoleLength { get; set; } = 180;

	/// <summary>
	/// What's the current hole, networked
	/// </summary>
	[HostSync]
	public Hole CurrentHole { get; set; }

	/// <summary>
	/// The starting hole for this course
	/// </summary>
	[Property] 
	public Hole StartingHole { get; set; }

	/// <summary>
	/// Game state
	/// </summary>
	[HostSync]
	public GameState State { get; set; } = GameState.WaitingForPlayers;
	
	/// <summary>
	/// The default game state
	/// </summary>
	[Property]
	public GameState DefaultState { get; set; } = GameState.WaitingForPlayers;

	/// <summary>
	/// While in play, each hole has a timer. If you don't putt in time, we move on.
	/// </summary>
	public TimeUntil TimeUntilNextHole { get; set; }

	/// <summary>
	/// Ran on update, check for any uncupped balls - if there are any, we won't end the hole.
	/// 
	/// This is easier than responding to states, but we might want to change it to that at some point.
	/// </summary>
	private void CheckRoundState()
	{
		if ( State != GameState.InPlay ) return;

		var waitingCount = Scene.GetAllComponents<Ball>()
			.Where( x => !x.IsCupped )
			.Count();
		
		if ( waitingCount == 0 )
			EndHole();
	}

	/// <summary>
	/// Ran on update, checks if we ran out of time on a hole.
	/// </summary>
	private void CheckNextHole()
	{
		if ( State == GameState.InPlay )
		{
			if ( TimeUntilNextHole )
			{
				Tell( "Time's up - moving to the next hole." );
				EndHole();
			}
		}
	}

	/// <summary>
	/// Tell a message to the whole session
	/// </summary>
	/// <param name="message"></param>
	[Broadcast]
	public void Tell( string message )
	{
		Scene.GetComponentInChildren<Chat>()
			.AddSystemText( message );
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
	/// Get a collection of the balls ordered by whoever's got the lowest par.
	/// </summary>
	/// <returns></returns>
	public IEnumerable<Ball> GetOrderedBalls()
	{
		return Scene.GetAllComponents<Ball>()
			.OrderByDescending( x => x.GetTotalPar() );
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

		State = GameState.HoleFinished;

		await GameTask.DelaySeconds( 3.0f );

		OpenScoreboard( true, 3.0f );

		await GameTask.DelaySeconds( 3.0f );

		Tell( "We're switching hole..." );

		CurrentHole = nextHole;
		State = GameState.NewHole;
		TimeUntilNextHole = HoleLength;

		if ( !CurrentHole.IsValid() )
			return;

		var position = CurrentHole.Start.WorldPosition + Vector3.Up * 16f;
		foreach ( var ball in Scene.GetAllComponents<Ball>() )
		{
			ball.IsCupped = false;
			ball.ResetPosition( position, Angles.Zero );
		}

		ShowHoleScreen( CurrentHole.Number, CurrentHole.Par );

		await GameTask.DelaySeconds( 5.0f );
		State = GameState.InPlay;
		TimeUntilNextHole = HoleLength;
	}

	[Broadcast]
	public void OpenScoreboard( bool open, float returnTime = 0f )
	{
		GolfScoreboard.Current.ForceOpen = open;

		if ( returnTime > 0f )
		{
			Invoke( returnTime, () => OpenScoreboard( false ) );
		}
	}

	public async void EndGame()
	{
		State = GameState.EndOfGame;
		
		await GameTask.DelaySeconds( 3f );

		Tell( "The game is over!" );
		BroadcastEndGame();
		OpenScoreboard( true, 15f );

		await GameTask.DelaySeconds( 15f );

		KillGame();
	}

	[Broadcast]
	private void KillGame()
	{
		Networking.Disconnect();
		Scene.Load( MenuSceneFile );
	}

	[Broadcast( NetPermission.HostOnly )]
	public void BroadcastEndGame()
	{
		IGameEvent.Post( x => x.OnGameOver() );
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

	// Should be a util
	private LegacyParticleSystem CreateParticleSystem( ParticleSystem particleSystem, Vector3 pos, float decay = 5f )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Name = $"Particle";
		gameObject.WorldPosition = pos;

		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.Particles = particleSystem;
		gameObject.Transform.ClearInterpolation();

		// Clear off in a suitable amount of time.
		p.Invoke( decay, gameObject.Destroy );

		return p;
	}

	void IGameEvent.OnGoal( Ball ball, HoleGoal goal )
	{
		Log.Info( $"{ball} hit {goal}" );

		// We the owner of the ball? We'd like to see the UI for it.
		if ( !ball.IsProxy )
		{
			ParScreen.Show( goal.Hole.Number, goal.Hole.Par, ball.GetCurrentPar() );

			if ( ball.GetCurrentPar() == 1 )
			{
				CreateParticleSystem( HoleInOneEffect, goal.WorldPosition + Vector3.Up * 16.0f );
			}
			else
			{
				CreateParticleSystem( ConfettiEffect, goal.WorldPosition + Vector3.Up * 16.0f );
			}

			Sound.Play( "minigolf.award" )
				.Volume = 0.5f;
		}

		Sound.Play( "minigolf.sink_into_cup", goal.WorldPosition );
	}

	// TODO: Use new particles
	[Property] 
	public ParticleSystem HoleInOneEffect { get; set; }

	// TODO: Use new particles
	[Property] 
	public ParticleSystem ConfettiEffect { get; set; }

	/// <summary>
	/// Show the hole screen UI to the person who did it
	/// </summary>
	/// <param name="hole"></param>
	/// <param name="par"></param>
	[Broadcast]
	private void ShowHoleScreen( int hole, int par )
	{
		NewHoleScreen.Show( hole, par );
	}
}
