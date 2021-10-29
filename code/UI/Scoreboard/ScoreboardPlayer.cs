using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minigolf
{
	public partial class ScoreboardPlayer : Panel
	{
		public Label PlayerName { get; set; }
		public Image PlayerAvatar { get; set; }
		public Panel ScoresPanel { get; set; }
		
		Label TotalScoreLabel { get; set; }

		public Client Client { get; private set; }

		Dictionary<int, Label> Scores = new();

		public ScoreboardPlayer( Client client, Panel panel ) : base( panel )
		{
			Client = client;
			SetClass( "me", Local.Client == Client );

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
				Scores[i + 1] = ScoresPanel.Add.Label( $"-" );
			}
		}

		public override void Tick()
		{
			for ( int i = 0; i < Game.Current.Course.Holes.Count; i++ )
			{
				Scores[i + 1].Text = $"{ Client.GetPar( i + 1 ) }";
			}

			TotalScoreLabel.Text = $"{ Client.GetTotalPar() }";
		}
	}
}
