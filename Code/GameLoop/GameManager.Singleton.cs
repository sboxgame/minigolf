namespace Facepunch.Minigolf;

public partial class GameManager
{
	public static GameManager Instance { get; private set; }

	public GameManager()
	{
		Instance = this;
	}
}
