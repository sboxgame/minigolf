namespace Facepunch.Minigolf.UI;

public class BaseMenu : Panel, INavigatorPage
{
	public BaseMenu()
	{
		AddClass( "menu" );
	}

	public void GoTo( MenuAttribute menu )
	{
		var name = "/" + menu.Name.ToLower();
		MainMenu.Current.GoTo( name );
	}
}
