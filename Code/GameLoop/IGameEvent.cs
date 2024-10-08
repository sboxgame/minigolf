public interface IGameEvent : ISceneEvent<IGameEvent>
{
	/// <summary>
	/// Called when we hit the ball
	/// </summary>
	/// <param name="ball"></param>
	void BallStroke( Ball ball ) { }

	/// <summary>
	/// Called when the ball switches from in play to not in play
	/// </summary>
	/// <param name="ball"></param>
	/// <param name="inPlay"></param>
	void BallInPlay( Ball ball, bool inPlay ) { }
}
