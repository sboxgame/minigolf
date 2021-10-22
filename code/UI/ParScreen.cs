
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minigolf
{
	public partial class ParScreen : Panel
	{
		public static readonly string HoleInOne = "Hole-in-One!";
		public static readonly Dictionary<int, string> ScoreText = new Dictionary<int, string>
		{
			{ 4, "Condor" },
			{ 3, "Double Eagle" },
			{ 2, "Eagle" },
			{ 1, "Birdie" },
			{ 0, "Par" },
			{ -1, "Bogey" },
			{ -2, "Double Bogey" },
			{ -3, "Triple Bogey" },
			{ -4, "Quadruple Bogey" },
			{ -5, "Quintuple Bogey" },
			{ -6, "Sextuple Bogey" },
			{ -7, "Septuple Bogey" },
			{ -8, "Octuple Bogey" },
			{ -9, "Nonuple Bogey" },
			{ -10, "Decuple Bogey" },
		};

		RealTimeSince CreatedAt;
		static float ShowScoreForLength => 3.0f;

		public ParScreen( int hole, int par, int strokes )
		{
			StyleSheet.Load( "/ui/ParScreen.scss" );

			Add.Label( ScoreText.GetValueOrDefault( par - strokes, $"WTF +{par - strokes}" ), "score" );
			Add.Label( $"Hole {hole}", "hole" );

			CreatedAt = 0;
		}

		public override void Tick()
		{
			if ( CreatedAt > ShowScoreForLength && !IsDeleting )
			{
				Delete();
			}
		}

		[ClientCmd( "minigolf_debug_testscore" )]
		public static void Show( int hole, int par, int strokes )
		{
			Local.Hud.AddChild( new ParScreen( hole, par, strokes ) );
		}
	}

}
