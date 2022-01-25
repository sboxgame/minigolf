using Sandbox;
using Sandbox.Hooks;
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
	public string MapName => Global.MapName;
	public string PlayerCount => Client.All.Count.ToString();
	public int CurrentHoleNumber => Game.Current.Course.CurrentHole.Number;
	public string CurrentHoleName => Game.Current.Course.CurrentHole.Name;

	public string TimerTimeLeft {
		get
		{
			if ( Game.Current.State == GameState.EndOfGame )
				return $"00:{ Math.Max( 0, (int)MathF.Floor( Game.Current.ReturnToLobbyTime - Time.Now ) ).ToString( "D2" ) }";
			if ( Game.Current.IsHoleEnding )
				return $"00:{ Math.Max( 0, (int)MathF.Floor( Game.Current.NextHoleTime - Time.Now ) ).ToString( "D2" ) }";
			return "";
		}
	}
	public string TimerTimeUntil
	{
		get
		{
			if ( Game.Current.State == GameState.EndOfGame )
				return "Return To Lobby";
			if ( Game.Current.IsHoleEnding )
				return "Next Hole";

			return "";
		}
	}

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
		if ( Game.Current.Course == null || Game.Current.Course.Holes.Count == 0 )
			return;

		HoleHeadersPanel.DeleteChildren( true );
		ParHeadersPanel.DeleteChildren( true );

		var holes = Game.Current.Course.Holes;
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
		SetClass( "timer--active", Game.Current.State == GameState.EndOfGame || Game.Current.IsHoleEnding );

		// Hacky hack
		if ( HoleHeadersPanel.Children.Count() == 0 )
		{
			PostTemplateApplied();

			return;
		}

		var holes = Game.Current.Course.Holes;
		for ( int i = 0; i < holes.Count; i++ )
		{
			var hole = HoleHeadersPanel.GetChild( i ) as Label;
			var par = ParHeadersPanel.GetChild( i ) as Label;

			hole.SetClass( "active", Game.Current.Course._currentHole == i );
			par.SetClass( "active", Game.Current.Course._currentHole == i );
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