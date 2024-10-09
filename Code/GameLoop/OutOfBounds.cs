[Title( "Hole Out Of Bounds" )]
public partial class OutOfBoundsArea : Component, Component.ITriggerListener
{
	/// <summary>
	/// When the ball enters this out of bounds area, how much time until we declare out of bounds?
	/// </summary>
	[Property( Title = "Forgiveness Time" )]
	public float ForgiveTime { get; set; } = 3f;

	public IEnumerable<Ball> TouchingBalls => touchingBalls;
	private readonly List<Ball> touchingBalls = new();

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		AddTouchingBall( ball );
		Facepunch.Minigolf.GameManager.Instance.UpdateBallInBounds( ball, false, ForgiveTime );
	}

	// materials/editor/minigolf_wall/minigolf_out_of_bounds.vmat

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		if ( touchingBalls.Contains( ball ) )
			touchingBalls.Remove( ball );
	}

	protected void AddTouchingBall( Ball ball )
	{
		if ( !ball.IsValid() )
			return;

		if ( !touchingBalls.Contains( ball ) )
			touchingBalls.Add( ball );
	}
}
