using Sandbox;
using System.Linq;

using Facepunch.Minigolf.Entities;
using Facepunch.Minigolf.UI;

namespace Facepunch.Minigolf;

public enum GameState
{
	WaitingForPlayers,
	Playing,
	EndOfGame
}

partial class MinigolfGame
{
	[Net, Change( nameof( OnStateChanged ) )]
	public GameState State { get; set; } = GameState.WaitingForPlayers;
	[Net] public float StartTime { get; private set; }
	[Net] public float ReturnToLobbyTime { get; private set; }
	[Net] public int LobbyCount { get; set; }

	public void OnStateChanged( GameState oldState, GameState newState )
	{
		Event.Run( MinigolfEvent.StateChange, newState );
	}

	public void StartGame()
	{
		State = GameState.Playing;

		// GameServices.StartGame();

		// Spawn balls for all clients
		foreach ( var cl in Sandbox.Game.Clients )
		{
			var ball = new Ball();
			cl.Pawn = ball;
			ball.Movement.ResetPosition( Course.CurrentHole.SpawnPosition, Course.CurrentHole.SpawnAngles );
		}
	}

	public void EndGame()
	{
		State = GameState.EndOfGame;

		//var clients = Sandbox.Game.Clients.OrderBy( cl => cl.GetTotalPar() ).ToList();
		//for ( int i = 0; i < clients.Count; i++ )
		//{
		//	// Don't score late comers
		//	if ( clients[i].GetValue<bool>( "late", false ) )
		//		continue;

		//	//var result = i == 0 ? GameplayResult.Win : GameplayResult.Lose;
		//	//clients[i].SetGameResult( result, clients[i].GetTotalPar() );
		//}

		////GameServices.EndGame();

		GolfScoreboard.SetOpen( To.Everyone, true );
		ReturnToLobbyTime = Time.Now + 15.0f;
	}

	[Net] public bool IsHoleEnding { get; private set; }
	[Net] public float NextHoleTime { get; private set; }

	public async void EndHole()
	{
		if ( IsHoleEnding )
			return;

		IsHoleEnding = true;
		NextHoleTime = Time.Now + 5.0f;

		// TODO: Is this the end of the game? Is there another hole after this?
		if ( Course.IsLastHole() )
		{
			EndGame();
			return;
		}

		GolfScoreboard.SetOpen( To.Everyone, true );
		await GameTask.DelaySeconds( 5.0f );
		GolfScoreboard.SetOpen( To.Everyone, false );

		// Set next hole
		Course.NextHole();
		// ChatBox.AddInformation( To.Everyone, "Show hole screen + camera" );
		// await GameTask.DelaySeconds( 5.0f );

		// Respawn all pawns
		foreach ( var cl in Sandbox.Game.Clients )
		{
			cl.Pawn = new Ball();
			(cl.Pawn as Ball).Movement.ResetPosition( Course.CurrentHole.SpawnPosition, Course.CurrentHole.SpawnAngles );
		}

		IsHoleEnding = false;
	}

	[GameEvent.Tick.Server]
	void ShouldStartGame()
	{
		if ( State != GameState.WaitingForPlayers ) return;
		if ( StartTime == 0 ) return; // Level not loaded yet

		if ( Time.Now >= StartTime || Sandbox.Game.Clients.Count >= LobbyCount )
			StartGame();

	}

	[GameEvent.Tick.Server]
	public void CheckRoundState()
	{
		if ( State != GameState.Playing ) return;
		if ( IsHoleEnding ) return;

		// Check if all playing clients have putted their ball
		var WaitingForClientsCount = Sandbox.Game.Clients.Count;
		foreach ( var cl in Sandbox.Game.Clients )
		{
			if ( !cl.Pawn.IsValid() )
				WaitingForClientsCount--;
		}

		if ( WaitingForClientsCount == 0 )
			EndHole();
	}

	[GameEvent.Tick.Server]
	public void ReturnToLobby()
	{
		if ( State != GameState.EndOfGame ) return;
		if ( Time.Now < ReturnToLobbyTime ) return;

		Sandbox.Game.Clients.ToList().ForEach( cl => cl.Kick() );
	}

	[ConCmd.Admin( "minigolf_force_start" )]
	public static void ForceStart()
	{
		Current.StartGame();
	}

	[ConCmd.Server( "minigolf_reset_ball" )]
	public static void ResetBall()
	{
		var client = ConsoleSystem.Caller;
		if ( !client.IsValid() ) return;
		if ( client.Pawn is not Ball ) return;

		Current.ResetBall( client );
	}

	[ConCmd.Admin( "minigolf_skip_to" )]
	public static void SkipToHole( int hole )
	{
		Current.Course.CurrentHoleIndex = hole;
		foreach ( var cl in Sandbox.Game.Clients )
		{
			Current.ResetBall( cl );
		}
	}
}
