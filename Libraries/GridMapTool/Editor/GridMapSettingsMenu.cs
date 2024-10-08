using Editor;

public static class GridMapSettingsMenu 
{
	[Menu( "Editor", "GridMapTool/Settings" )]
	public static void OpenMyMenu()
	{
		new GridMapSettings();

	}
}

public class GridMapSettings : BaseWindow
{
	public GridMapSettings()
	{
		WindowTitle = "GridMap Settings";
		SetWindowIcon( "settings" );
		Name = "GridMap Settings";
		Size = new Vector2( 400, 700 );

		defaultHeight = ProjectCookie.Get<float>( "GridHeight", 128 );
		defaultGridMultiplier = ProjectCookie.Get<float>( "GridMultiplier", 1.0f );

		CreateUI();
		Show();


	}

	Widget container;

	float defaultHeight = 128;
	float newHeight = 128;
	float defaultGridMultiplier = 1.0f;
	float newGridMultiplier = 1.0f;

	public void CreateUI()
	{
		Layout = Layout.Column();
		Layout.Margin = 4;
		Layout.Spacing = 4;

		container = new Widget( this );

		var properties = new PropertySheet( this );
		var nameLabel = new Label.Subtitle( "Grid Map Settings" );
		nameLabel.Margin = 16;

		var _defaultheight = properties.AddRow( "Default Height", new LineEdit( "Default Height" ) );
		_defaultheight.Text = defaultHeight.ToString();
		_defaultheight.Bind("Value").From( () => newHeight.ToString(), x => { if ( float.TryParse( x, out var f ) ) newHeight = f; } );

		var _defaultGridMultiplier = properties.AddRow( "Default Grid Multiplier", new LineEdit( "Default Grid Multiplier" ) );
		_defaultGridMultiplier.Text = defaultGridMultiplier.ToString();
		_defaultGridMultiplier.Bind("Value").From( () => newGridMultiplier.ToString(), x => { if ( float.TryParse( x, out var f ) ) newGridMultiplier = f; } );


		var saveButton = new Button.Primary( "Save Settings", "add_circle" );
		saveButton.Clicked = () => SaveSettings();

		Layout.Add( nameLabel );
		Layout.Add( properties );
		Layout.Add( saveButton );
		Layout.Add(container);
	}

	public void SaveSettings()
	{
		// Save settingsPackage
		ProjectCookie.Set<float>( "GridHeight", newHeight );
		ProjectCookie.Set<float>( "GridMultiplier", newGridMultiplier );
	}
}

public class GridMapCookie
{

}
