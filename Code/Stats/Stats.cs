namespace Facepunch.Minigolf;

public static class Stats
{
	/// <summary>
	/// Parses a stat's ident, because we might want to store stats per-course.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="useCourse"></param>
	/// <returns></returns>
	public static string ParseIdent( string name, bool useCourse = true )
	{
		if ( useCourse ) return $"{name}-{Course.CurrentInfo.Ident}";
		return name;
	}

	/// <summary>
	/// Get a stat
	/// </summary>
	/// <param name="ident"></param>
	/// <param name="useCourse"></param>
	/// <returns></returns>
	public static int Get( string ident, bool useCourse = true )
	{
		ident = ParseIdent( ident, useCourse );
		return (int)Sandbox.Services.Stats.LocalPlayer.Get( ident ).Value;
	}

	/// <summary>
	/// Get an average stat
	/// </summary>
	/// <param name="ident"></param>
	/// <param name="useCourse"></param>
	/// <returns></returns>
	public static int GetAverage( string ident, bool useCourse = true )
	{
		ident = ParseIdent( ident, useCourse );
		return (int)Sandbox.Services.Stats.LocalPlayer.Get( ident ).Avg;
	}

	/// <summary>
	/// Increments a stat.
	/// </summary>
	/// <param name="ident"></param>
	/// <param name="amt"></param>
	/// <param name="useCourse"></param>
	public static void Increment( string ident, int amt = 1, bool useCourse = true )
	{
		ident = ParseIdent( ident, useCourse );
		Sandbox.Services.Stats.Increment( ident, amt );

		if ( Game.IsEditor )
		{
			FlushAsync();
		}
	}

	/// <summary>
	/// Flushes the stats, used in the editor only for dev purposes.
	/// </summary>
	private async static void FlushAsync()
	{
		await Sandbox.Services.Stats.FlushAndWaitAsync();
	}
}
