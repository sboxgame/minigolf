namespace Facepunch.Minigolf.UI;

public class BaseMenu : Panel, INavigatorPage
{
	public BaseMenu()
	{
		AddClass( "menu" );
	}

	public void GoTo( Type menu )
	{
		var name = "/" + menu.Name.ToLower();

		Log.Info( "Navigating to " + name );

		this.Delete();
		this.Navigate( name );
	}
}
