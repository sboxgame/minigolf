namespace Facepunch.Minigolf;

public partial class GameManager
{
	[ConVar( "minigolf_check_bounds" )]
	public static bool CheckBounds { get; set; } = true;

	[ConVar( "minigolf_bounds_forgiveness" )]
	public static float BoundsForgiveness { get; set; } = 3.0f;

	public enum OutOfBoundsType
	{
		Normal,
		Water,
		Fire
	}

	Dictionary<Ball, float> OutOfBoundsBalls = new();

	public void UpdateBallInBounds( Ball ball, bool inBounds, float forgiveTime = 0 )
	{
		if ( inBounds && OutOfBoundsBalls.ContainsKey( ball ) )
		{
			OutOfBoundsBalls.Remove( ball );
		}

		if ( !inBounds && !OutOfBoundsBalls.ContainsKey( ball ) )
		{
			OutOfBoundsBalls[ball] = Time.Now + forgiveTime;
		}
	}

	private void CheckBoundsTimes()
	{
		var copy = new Dictionary<Ball, float>( OutOfBoundsBalls );
		foreach ( var ball in copy.Keys )
		{
			// TODO: check cupped balls
			if ( !ball.IsValid() /*|| ball.Cupped*/ )
			{
				OutOfBoundsBalls.Remove( ball );
				continue;
			}

			var time = copy[ball] + BoundsForgiveness;
			if ( Time.Now > time )
			{
				OutOfBoundsBalls.Remove( ball );
				BallOutOfBounds( ball, OutOfBoundsType.Normal );
			}
		}
	}

	public void BallOutOfBounds( Ball ball, OutOfBoundsType type )
	{
		if ( IsProxy )
			return;

		// Ball is OOB, move it back to its non-OOB location
		ball.ResetPosition();
	}
}
