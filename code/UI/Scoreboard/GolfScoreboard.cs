namespace Facepunch.Minigolf.UI;

public partial class GolfScoreboard : Panel
{
	[ClientRpc]
	public static void SetOpen( bool open )
	{
		if ( Current != null )
			Current.ForceOpen = open;
	}
}
