namespace Facepunch.Minigolf;

public static class ScoreUtils
{
	public static readonly Dictionary<int, string> ScoreText = new()
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

	/// <summary>
	/// Get the score name
	/// </summary>
	/// <param name="par"></param>
	/// <param name="score"></param>
	/// <returns></returns>
	public static string GetScore( int par, int score )
	{
		if ( score < 1 ) return "Hole-In-One"; 

		return ScoreText.GetValueOrDefault( par - score, $"{par - score} Over Par" );
	}
}
