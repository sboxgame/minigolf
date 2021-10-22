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

			public ScoreFeedEntry( Panel parent, Client cl, int score ) : base( parent )
			{
				Add.Image( $"avatarbig:{cl.SteamId}", "avatar" );
				Add.Label( ParScreen.ScoreText.GetValueOrDefault( score, $"{score} Over Par" ), "score" );
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

		public void AddEntry( Client cl, int score )
		{
			_ = new ScoreFeedEntry( this, cl, score );
		}

		[ClientCmd("minigolf_score_test")]
		public static void AddTest()
		{
			Instance.AddEntry( Local.Client, 3 );
		}
	}
}
