global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Sandbox.Component;
global using Editor;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.ComponentModel;
global using System.Threading.Tasks;
using Facepunch.Minigolf.Entities;
using Facepunch.Minigolf.UI;
using Facepunch.Customization;

namespace Facepunch.Minigolf;

public partial class MinigolfGame : Sandbox.GameManager
{
	public static new MinigolfGame Current => Sandbox.GameManager.Current as MinigolfGame;

	public MinigolfGame()
	{
		if ( Game.IsServer )
		{
			AddToPrecache();
			Course = new Course();
		}

		if ( Game.IsClient )
			Game.RootPanel = new GolfRootPanel();
	}

	[Event.Hotload]
	private void OnHotload()
	{
		// sometings fucked with BaseNetworkable during hotload, data gets lost
		// just make a new Course for now to avoid nre spam
		if ( !Game.IsClient ) Course.LoadFromMap();
	}

	public override void ClientJoined( IClient cl )
	{
		Log.Info( $"\"{cl.Name}\" has joined the game" );

		cl.Components.Create<CustomizationComponent>();

		if ( State == GameState.Playing )
		{
			cl.SetValue( "late", true );
			TextChat.AddInfoChatEntry( To.Everyone, $"{cl.Name} has joined late, they will not be eligible for scoring." );

			// Just give them shitty scores on each hole for now
			for ( int i = 0; i <= Course.CurrentHoleIndex; i++ )
				cl.SetInt( $"par_{i}", Course.Holes[i].Par + 1 );
		}
		else
		{
			TextChat.AddInfoChatEntry( To.Everyone, $"{cl.Name} has joined" );
		}
	}

	public override bool CanHearPlayerVoice( IClient source, IClient dest ) => true;

	public override void OnVoicePlayed( IClient source )
	{
		VoiceChat.OnVoiceChat( source );
	}

	public override void PostLevelLoaded()
	{
		StartTime = Time.Now + 60.0f;
		Course.LoadFromMap();
	}

	public override void BuildInput()
	{
		Game.AssertClient();

		// todo: pass to spectate
		var ball = Game.LocalPawn as Ball;

		if ( Input.Pressed( InputActions.View ) && Game.LocalPawn.IsValid() && ball.CanUseFreeCamera() )
			FreeCamera = FreeCamera == null ? Components.GetOrCreate<FreeCamera>() : null;

		var camera = FindActiveCamera();
		camera?.BuildInput();
		camera?.Update();
		ball?.BuildInput();
	}

	[MinigolfEvent.NextHole]
	private void OnHoleChange( int hole )
	{
		ResetFreeCamera( To.Everyone );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		if ( cl.Pawn is not Ball ball )
			return;

		if ( !ball.Cupped && Input.Pressed( InputActions.Reload ) )
			ResetBall( cl );
	}
}
