using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Minigolf.UI;

public partial class WaitingScreenClient : Panel
{		
	public IClient Client { get; set; }
	public Image Avatar { get; set; }
	public bool Loaded { get; set; }

	public WaitingScreenClient( IClient cl )
	{
		Client = cl;
		Avatar = Add.Image( $"avatarbig:{cl.SteamId}" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Client.IsValid() )
			return;

		SetClass( "loaded", Loaded );
	}
}

[UseTemplate]
public partial class WaitingScreen : Panel
{
	Panel PlayersContainer { get; set; }

	// Bindables for HTML:
	public string StartingTimeLeft => $"{ Math.Max(0, MinigolfGame.Current.StartTime - Time.Now ).CeilToInt() }";
	public string PlayerCount => $"{Sandbox.Game.Clients.Count}";
	public string MaxPlayers => $"{ MinigolfGame.Current.LobbyCount }";

	public override void Tick()
	{
		// Remove any invalid clients (disconnected)
		foreach ( var panel in PlayersContainer.Children.OfType<WaitingScreenClient>() )
		{
			if ( panel.Client.IsValid() ) continue;
			panel.Delete();
		}

		// Add any new clients that aren't already in the list
		foreach ( var client in Sandbox.Game.Clients )
		{
			if ( PlayersContainer.Children.OfType<WaitingScreenClient>().Any( panel => panel.Client == client ) ) continue;
			PlayersContainer.AddChild( new WaitingScreenClient( client ) );
		}
	}
}
