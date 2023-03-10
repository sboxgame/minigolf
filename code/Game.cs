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

public partial class Game : Sandbox.GameManager
{
	public static new Game Current => Sandbox.GameManager.Current as Game;

	public Game()
	{
		if ( Sandbox.Game.IsServer )
		{
			AddToPrecache();
			Course = new();
		}

		if ( Sandbox.Game.IsClient )
		{
			Sandbox.Game.RootPanel = new GolfRootPanel();
		}

		Customize.WatchForChanges = true;
	}

	[Event.Hotload]
	private void OnHotload()
    {
		// sometings fucked with BaseNetworkable during hotload, data gets lost
		// just make a new Course for now to avoid nre spam
		if ( Sandbox.Game.IsClient ) return;
		Course.LoadFromMap();
    }

	public override void ClientJoined( IClient cl )
	{
		Log.Info( $"\"{cl.Name}\" has joined the game" );

		cl.Components.Create<CustomizeComponent>();

		if ( State == GameState.Playing )
		{
			cl.SetValue( "late", true );
			Facepunch.Minigolf.UI.ChatBox.AddInformation( To.Everyone, $"{ cl.Name } has joined late, they will not be eligible for scoring.", $"avatar:{ cl.SteamId }" );

			// Just give them shitty scores on each hole for now
			for ( int i = 0; i <= Course._currentHole; i++ )
				cl.SetInt( $"par_{ i }", Course.Holes[i].Par + 1 );
		}
		else
		{
			Facepunch.Minigolf.UI.ChatBox.AddInformation( To.Everyone, $"{ cl.Name } has joined", $"avatar:{ cl.SteamId }" );
		}
	}

	public override bool CanHearPlayerVoice( IClient source, IClient dest ) => true;

	public override void PostLevelLoaded()
	{
		StartTime = Time.Now + 60.0f;
		Course.LoadFromMap();
	}

	public override void BuildInput()
	{
		Sandbox.Game.AssertClient();

		Event.Run( "buildinput" );

		// todo: pass to spectate
		var ball = Sandbox.Game.LocalPawn as Ball;

		if ( Input.Pressed( InputButton.View ) && Sandbox.Game.LocalPawn.IsValid() && !ball.InPlay && !ball.Cupped && FreeCamTimeLeft > 0.0f )
		{
			if ( FreeCamera == null )
				FreeCamera = Components.GetOrCreate<FreeCamera>();
			else
				FreeCamera = null;
		}

		// the camera is the primary method here
		var camera = FindActiveCamera();
		camera?.BuildInput();
		camera?.Update();

		ball?.BuildInput();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( cl.Pawn is Ball ball && !ball.Cupped )
		{
			if ( Input.Pressed( InputButton.Reload ) )
				ResetBall( cl );
		}
	}
}
