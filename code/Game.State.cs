using Sandbox;
using System.Collections.Generic;

namespace Minigolf
{
	public enum GameState
	{
		WaitingForPlayers,
		Playing,
		EndOfGame
	}

	partial class Game
	{
		[Net, Change( nameof( OnStateChanged ) )]
		public GameState State { get; set; } = GameState.WaitingForPlayers;

		[Net] public List<Client> PlayingClients { get; set; }

		public void OnStateChanged( GameState oldState, GameState newState )
		{
			Event.Run( "minigolf.state.changed", newState );
			// Pass to HUD?
		}

		[ServerCmd]
		public static void Ready()
		{
			var client = ConsoleSystem.Caller;
			if ( client == null ) return;

			client.SetValue( "ready", !client.GetValue<bool>( "ready", false ) );

			// TODO: move this to a tick or some shit
			foreach ( var cl in Client.All )
			{
				if ( !cl.GetValue<bool>( "ready", false ) )
					return;
			}

			Current.StartGame();
		}

		public void StartGame()
		{
			PlayingClients = new List<Client>();
			foreach( var client in Client.All )
			{
				if ( client.GetValue<bool>( "ready", false ) )
					PlayingClients.Add( client );
			}

			// Spawn balls for all clients
			foreach ( var cl in PlayingClients )
			{
				var ball = new Ball();
				cl.Pawn = ball;
				ball.ResetPosition( Course.CurrentHole.SpawnPosition, Course.CurrentHole.SpawnAngles );
			}

			State = GameState.Playing;
		}

		bool IsEnding;

		public async void EndHole()
		{
			if ( IsEnding )
				return;

			IsEnding = true;

			// TODO: Is this the end of the game? Is there another hole after this?

			GolfScoreboard.SetOpen( To.Everyone, true );
			await GameTask.DelaySeconds( 5.0f );
			GolfScoreboard.SetOpen( To.Everyone, false );

			// Set next hole
			Course.NextHole();
			// ChatBox.AddInformation( To.Everyone, "Show hole screen + camera" );
			// await GameTask.DelaySeconds( 5.0f );

			// Respawn all pawns
			foreach ( var cl in PlayingClients )
			{
				cl.Pawn = new Ball();
				(cl.Pawn as Ball).ResetPosition( Course.CurrentHole.SpawnPosition, Course.CurrentHole.SpawnAngles );
			}

			IsEnding = false;
		}

		[Event.Tick.Server]
		public void CheckRoundState()
		{
			switch ( State )
			{
				case GameState.WaitingForPlayers:
					// TODO: Check Ready Clients
					break;
				case GameState.Playing:
					if ( IsEnding )
						break;

					// Check if all playing clients have putted their ball
					var WaitingForClientsCount = PlayingClients.Count;
					foreach ( var cl in PlayingClients )
					{
						if ( !cl.Pawn.IsValid() )
							WaitingForClientsCount--;
					}

					if ( WaitingForClientsCount != 0 )
					{
						break;
					}

					EndHole();

					break;
				case GameState.EndOfGame:
					break;
			}
		}

		[AdminCmd( "minigolf_force_start" )]
		public static void ForceStart()
		{
			// Force everyone to ready
			foreach ( var client in Client.All )
			{
				client.SetValue( "ready", true );
			}

			Current.StartGame();
		}

		[ServerCmd( "minigolf_reset_ball" )]
		public static void ResetBall()
		{
			var client = ConsoleSystem.Caller;
			if ( !client.IsValid() ) return;
			if ( client.Pawn is not Ball ) return;

			Current.ResetBall( client );
		}

		[AdminCmd( "minigolf_skip_to" )]
		public static void SkipToHole( int hole )
		{
			Current.Course._currentHole = hole;
			foreach ( var cl in Current.PlayingClients )
			{
				Current.ResetBall( cl );
			}
		}
	}
}
