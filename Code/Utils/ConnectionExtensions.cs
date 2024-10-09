namespace Facepunch.Minigolf;

public static class ConnectionExtensions
{
	/// <summary>
	/// Get the connection's ball
	/// </summary>
	/// <param name="self"></param>
	/// <returns></returns>
	public static Ball GetBall( this Connection self )
	{
		return Game.ActiveScene.GetAllComponents<Ball>().FirstOrDefault( x => x.Network.Owner == self );
	}
}
