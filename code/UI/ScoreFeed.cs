using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Minigolf
{
	public partial class ScoreFeed : Panel
	{
		public static ScoreFeed Instance;

		public class ScoreFeedEntry : Panel
		{
			public RealTimeSince AddedTimeSince;

			public ScoreFeedEntry( Panel parent, Client cl, int par, int score ) : base( parent )
			{
				// todo: hole in one

				Add.Image( $"avatarbig:{cl.PlayerId}", "avatar" );
				if ( score == 1 )
				{
					Add.Label( "Hole-in-One", "score holeinone" );
				}
				else
				{
					Add.Label( ParScreen.ScoreText.GetValueOrDefault( par - score, $"{ par - score } Over Par" ), "score" );
				}
				Add.Label( $"scored" );
				Add.Label( $"{cl.Name}", "name" );

				AddedTimeSince = 0;
			}

			public override void Tick()
			{
				if ( AddedTimeSince > 5.0f && !IsDeleting )
				{
					Delete();
				}
			}
		}

		public ScoreFeed()
		{
			Instance = this;

			StyleSheet.Load( "/ui/ScoreFeed.scss" );
		}

		public void AddEntry( Client cl, int par, int score )
		{
			_ = new ScoreFeedEntry( this, cl, par, score );
			Sound.FromScreen( "minigolf.award" ).SetVolume( 0.5f );
		}

		[ClientCmd("minigolf_score_test")]
		public static void AddTest()
		{
			Instance.AddEntry( Local.Client, 3, 3 );
			Sound.FromScreen( "minigolf.award" ).SetVolume(0.5f);
		}
	}
}
