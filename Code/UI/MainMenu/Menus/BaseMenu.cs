namespace Facepunch.Minigolf.UI;

/// <summary>
/// Called from the main menu
/// </summary>
public interface IMainMenuEvents
{
	/// <summary>
	/// Called when we navigate from page to page.
	/// </summary>
	/// <param name="url"></param>
	public void OnNavigated( string url );
}

public class BaseMenu : Panel, INavigatorPage
{
	public BaseMenu()
	{
		AddClass( "menu" );
	}

	void INavigatorPage.OnNavigationOpen()
	{
		var hostPanel = AncestorsAndSelf.OfType<NavHostPanel>().FirstOrDefault();
		var url = hostPanel.CurrentUrl;

		ISceneEvent<IMainMenuEvents>.Post( x => x.OnNavigated( url ) );
	}
}
