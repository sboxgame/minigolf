using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Minigolf.UI;

public partial class ScoreboardPlayer : Panel
{
	public Label PlayerName { get; set; }
	public Image PlayerAvatar { get; set; }
	public Panel ScoresPanel { get; set; }
		
	Label TotalScoreLabel { get; set; }

	public IClient Client { get; private set; }

	Dictionary<int, Label> Scores = new();

	public ScoreboardPlayer( IClient client, Panel panel ) : base( panel )
	{
		Client = client;
		SetClass( "me", Sandbox.Game.LocalClient == Client );

		SetTemplate( "/UI/Scoreboard/ScoreboardPlayer.html" );
	}

	protected override void PostTemplateApplied()
	{
		PlayerName.Text = Client.Name;
		PlayerAvatar.Texture = Texture.Load( $"avatar:{Client.SteamId}" );

		foreach ( var pnl in Scores.Values )
			pnl.Delete();

		Scores.Clear();

		for ( int i = 0; i < Game.Current.Course.Holes.Count; i++ )
		{
			Scores[i] = ScoresPanel.Add.Label( $"-" );
		}
	}

	public override void Tick()
	{
		for ( int i = 0; i < Game.Current.Course.Holes.Count; i++ )
		{
			if ( Game.Current.Course._currentHole < i )
				continue;

			var par = Client.GetPar( i );
			var holePar = Game.Current.Course.Holes[i].Par;

			Scores[i].Text = $"{ par }";
			Scores[i].SetClass( "active", Game.Current.Course._currentHole == i );
			Scores[i].SetClass( "below", par < holePar );
			Scores[i].SetClass( "over", par > holePar );
		}

		TotalScoreLabel.Text = $"{ Client.GetTotalPar() }";
	}
}
