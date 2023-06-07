using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Minigolf.UI;


[UseTemplate]
public partial class GolfScoreboard : Panel
{
	static GolfScoreboard Current;

	public bool ForceOpen;

	// Bindings for HTML
	public string MapName => Sandbox.Game.Server.MapIdent;
	public string PlayerCount => Sandbox.Game.Clients.Count.ToString();
	public int CurrentHoleNumber => MinigolfGame.Current.Course.CurrentHole.Number;
	public string CurrentHoleName => MinigolfGame.Current.Course.CurrentHole.Name;

	public string TimerTimeLeft {
		get
		{
			if ( MinigolfGame.Current.State == GameState.EndOfGame )
				return $"00:{ Math.Max( 0, (int)MathF.Floor( MinigolfGame.Current.ReturnToLobbyTime - Time.Now ) ).ToString( "D2" ) }";
			if ( MinigolfGame.Current.IsHoleEnding )
				return $"00:{ Math.Max( 0, (int)MathF.Floor( MinigolfGame.Current.NextHoleTime - Time.Now ) ).ToString( "D2" ) }";
			return "";
		}
	}
	public string TimerTimeUntil
	{
		get
		{
			if ( MinigolfGame.Current.State == GameState.EndOfGame )
				return "Return To Lobby";
			if ( MinigolfGame.Current.IsHoleEnding )
				return "Next Hole";

			return "";
		}
	}

	Panel PlayersPanel { get; set; }
	Panel HoleHeadersPanel { get; set; }
	Panel ParHeadersPanel { get; set; }

	Dictionary<IClient, ScoreboardPlayer> Players = new();

	public GolfScoreboard()
	{
		Current = this;
	}

	protected override void PostTemplateApplied()
	{
		if ( MinigolfGame.Current.Course == null || MinigolfGame.Current.Course.Holes.Count == 0 )
			return;

		HoleHeadersPanel.DeleteChildren( true );
		ParHeadersPanel.DeleteChildren( true );

		var holes = MinigolfGame.Current.Course.Holes;
		for ( int i = 0; i < holes.Count; i++ )
		{
			HoleHeadersPanel.Add.Label( $"{ holes[i].Number }" );
			ParHeadersPanel.Add.Label( $"{ holes[i].Par }" );
		}

		// If this is from a hot reload, clear any existing names
		foreach ( var pnl in Players.Values )
			pnl.Delete();

		Players.Clear();
	}

	public override void Tick()
	{
		// Some shitty code
		SetClass( "timer--active", MinigolfGame.Current.State == GameState.EndOfGame || MinigolfGame.Current.IsHoleEnding );

		// Hacky hack
		if ( HoleHeadersPanel.Children.Count() == 0 )
		{
			PostTemplateApplied();

			return;
		}

		var holes = MinigolfGame.Current.Course.Holes;
		for ( int i = 0; i < holes.Count; i++ )
		{
			var hole = HoleHeadersPanel.GetChild( i ) as Label;
			var par = ParHeadersPanel.GetChild( i ) as Label;

			hole.SetClass( "active", MinigolfGame.Current.Course._currentHole == i );
			par.SetClass( "active", MinigolfGame.Current.Course._currentHole == i );
		}

		if ( ForceOpen )
			SetClass( "open", true );
		else if ( MinigolfGame.Current.State == GameState.Playing )
			SetClass( "open", Input.Down( "score" ) );

		foreach ( var client in Sandbox.Game.Clients.Except( Players.Keys ) )
		{
			var entry = AddClient( client );
			Players[client] = entry;
		}

		foreach ( var client in Players.Keys.Except( Sandbox.Game.Clients ) )
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

	protected ScoreboardPlayer AddClient( IClient cl )
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
