using Facepunch.Minigolf.UI;
using Sandbox;

namespace Facepunch.Minigolf;

public enum GameState
{
	WaitingForPlayers,
	InPlay,
	EndOfGame
}

public partial class GameManager
{
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
	/// Are we ending the hole, do cinematic camera etc - this should be GameState?
	/// </summary>
	[HostSync]
	public bool IsHoleEnding { get; set; }

	/// <summary>
	/// Game state
	/// </summary>
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

		ball.IsCupped = true;
		Sound.Play( "minigolf.sink_into_cup", goal.WorldPosition );

		using ( Rpc.FilterInclude( ball.Network.Owner ) )
		{
			ShowUI( goal.Hole.Number, goal.Hole.Par, ball.GetCurrentPar() );
		}

		BroadcastEffects( ball, goal );

		Sound.Play( "minigolf.award" ).Volume = 0.5f;
	}

	[Broadcast]
	private void BroadcastEffects( Ball ball, HoleGoal goal )
	{
		if ( ball.GetCurrentPar() == 1 )
		{
			CreateParticleSystem( ParticleSystem.Load( "particles/gameplay/hole_in_one/v2/hole_in_one.vpcf" ), goal.WorldPosition + Vector3.Up * 16.0f );
		}
		else
		{
			CreateParticleSystem( ParticleSystem.Load( "particles/gameplay/hole_effect/confetti.vpcf" ), goal.WorldPosition + Vector3.Up * 16.0f );
		}
	}

	/// <summary>
	/// Show the par screen UI to the person who did it
	/// </summary>
	/// <param name="hole"></param>
	/// <param name="par"></param>
	/// <param name="strokes"></param>
	[Broadcast]
	private void ShowUI( int hole, int par, int strokes )
	{
		ParScreen.Show( hole, par, strokes );
	}
}
