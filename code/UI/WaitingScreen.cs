using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Minigolf
{
	public partial class WaitingScreenClient : Panel
	{		
		public Client Client { get; set; }
		public Image Avatar { get; set; }
		public bool Loaded { get; set; }

		public WaitingScreenClient( Client cl )
		{
			Client = cl;
			Avatar = Add.Image( $"avatarbig:{cl.PlayerId}" );
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
		public string StartingTimeLeft => $"{ Math.Max(0, Game.Current.StartTime - Time.Now ).CeilToInt() }";
		public string PlayerCount => $"{Client.All.Count}";
		public string MaxPlayers => $"{ Game.Current.LobbyCount }";

		public override void Tick()
		{
			// Remove any invalid clients (disconnected)
			foreach ( var panel in PlayersContainer.Children.OfType<WaitingScreenClient>() )
			{
				if ( panel.Client.IsValid() ) continue;
				panel.Delete();
			}

			// Add any new clients that aren't already in the list
			foreach ( var client in Client.All )
			{
				if ( PlayersContainer.Children.OfType<WaitingScreenClient>().Any( panel => panel.Client == client ) ) continue;
				PlayersContainer.AddChild( new WaitingScreenClient( client ) );
			}
		}
	}
}
