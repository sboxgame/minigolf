using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minigolf
{
	[UseTemplate]
	public partial class GolfScoreboard : Panel
	{
		static GolfScoreboard Current;

		public bool ForceOpen;

		// Bindings for HTML
		public string MapName => Global.MapName;
		public string PlayerCount => Client.All.Count.ToString();
		public int CurrentHoleNumber => Game.Current.Course.CurrentHole.Number;
		public string CurrentHoleName => Game.Current.Course.CurrentHole.Name;

		Panel PlayersPanel { get; set; }
		Panel HoleHeadersPanel { get; set; }
		Panel ParHeadersPanel { get; set; }

		Dictionary<Client, ScoreboardPlayer> Players = new();

		public GolfScoreboard()
		{
			Current = this;
		}

		protected override void PostTemplateApplied()
		{
			if ( Game.Current.Course == null )
				return;

			HoleHeadersPanel.DeleteChildren( true );
			ParHeadersPanel.DeleteChildren( true );

			for ( int i = 0; i < Game.Current.Course.Holes.Count; i++ )
			{
				HoleHeadersPanel.Add.Label( $"{ i + 1 }" );
				ParHeadersPanel.Add.Label( $"3" );
			}

			// If this is from a hot reload, clear any existing names
			foreach ( var pnl in Players.Values )
				pnl.Delete();

			Players.Clear();
		}

		public override void Tick()
		{
			// Hacky hack
			if ( HoleHeadersPanel.Children.Count() == 0 )
			{
				if ( Game.Current.Course != null )
				{
					PostTemplateApplied();
				}

				return;
			}

			for ( int i = 0; i < Game.Current.Course.Holes.Count; i++ )
			{
				var hole = HoleHeadersPanel.GetChild( i ) as Label;
				var par = ParHeadersPanel.GetChild( i ) as Label;
				hole.SetClass( "active", Game.Current.Course.CurrentHole.Number == i + 1 );
				par.SetClass( "active", Game.Current.Course.CurrentHole.Number == i + 1 );
			}

			if ( ForceOpen )
				SetClass( "open", true );
			else if ( Game.Current.State == GameState.Playing )
				SetClass( "open", Input.Down( InputButton.Score ) );

			foreach ( var client in Client.All.Except( Players.Keys ) )
			{
				var entry = AddClient( client );
				Players[client] = entry;
			}

			foreach ( var client in Players.Keys.Except( Client.All ) )
			{
				if ( Players.TryGetValue( client, out var row ) )
				{
					row?.Delete();
					Players.Remove( client );
				}
			}

			// TODO: Only sort when we need to... OnScoreUpdated event?
			PlayersPanel.SortChildren<ScoreboardPlayer>( ( p ) => p.Client.GetTotalPar() );
		}

		protected ScoreboardPlayer AddClient( Client cl )
		{
			var pnl = new ScoreboardPlayer( cl, PlayersPanel );
			PlayersPanel.AddChild( pnl );
			return pnl;
		}

		[ClientRpc]
		public static void SetOpen( bool open )
		{
			if ( Current != null )
				Current.ForceOpen = open;
		}
	}
}
