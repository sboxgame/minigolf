using Sandbox.UI;
using System;
using System.Drawing;
using System.Net.WebSockets;
using static Sandbox.Gizmo;

namespace Editor;

public partial class GridMapTool
{
	private Label floorLabel;
	private Label floorcontrolLabel;
	private Label rotationLabel;
	private Label selectedamount;
	private Label currentaxisLabel;

	private LineEdit heightInput;
	private WidgetWindow gridwindowWidget;
	private FloatSlider slider;

	private WidgetWindow popup;
	private ComboBox rotationSnapBox;
	private WidgetWindow window;

	public ComboBox collectionDropDown { get; set; } = new();
	public ComboBox groupDropDown { get; set; } = new();
	[Sandbox.Range( 0, 100 )]
	public float thumbnailScale { get; set; }

	public GroundAxis Axis { get; set; } = GroundAxis.Z;
	public enum GroundAxis
	{

		X,
		Y,
		Z
	}

	ListStyle CurrentListStyle = ListStyle.Grid;
	private SegmentedControl paintmode;
	private SerializedProperty resourceso;

	public enum ListStyle
	{
		Grid,
		List
	}
	public float[] RotationSnaps = new float[] { 1.0f, 5.0f, 15.0f, 45.0f, 90.0f };

	void ToolWindow( SerializedObject so )
	{

		{
			gridwindowWidget = new WidgetWindow( SceneOverlay );
			gridwindowWidget.MaximumWidth = 300;
			gridwindowWidget.WindowTitle = "Grid Map Tool";

			var row = Layout.Column();

			var collectionrow = Layout.Row();

			var collectionLabel = new Label( "Collection :" );
			var collectionCombo = collectionDropDown;

			var grouprow = Layout.Row();
			var groupLabel = new Label( "Groups :" );
			var groupCombo = groupDropDown;

			groupCombo.ItemChanged += () =>
			{
				UpdateListViewItems();
			};

			grouprow.Add( groupLabel );
			grouprow.Add( groupCombo );

			var collectionButton = new Button( "", "add" );
			collectionButton.ButtonType = "clear";
			collectionButton.Clicked = () =>
			{
				var popup = new NewCollectionObjectWindow();
				popup.Show();
				popup.gridmapToolDockWidget = this;
			};
			collectionButton.MaximumWidth = 32;

			collectionrow.AddSpacingCell( 5 );
			collectionrow.Add( collectionLabel );
			collectionrow.AddSpacingCell( 1 );
			collectionrow.Add( collectionCombo );
			collectionrow.Add( collectionButton );

			var iconrow = Layout.Row();

			var search = new LineEdit();
			search.PlaceholderText = "Search...";
			search.TextEdited += OnSearchTextChanged;
			iconrow.Add( search );

			slider = iconrow.Add( new FloatSlider( gridwindowWidget ) );
			slider.Minimum = 48;
			slider.Maximum = 128;
			slider.MinimumWidth = 150;
			slider.Value = 64;

			iconrow.AddSpacingCell( 2 );
			var buttonGrid = iconrow.Add( new Button( "", "grid_view" ) );
			buttonGrid.OnPaintOverride += () =>
			{
				PaintButton( "grid_view", ListStyle.Grid );
				return true;
			};

			buttonGrid.MaximumSize = 24;
			var buttonList = iconrow.Add( new Button() );
			buttonList.MaximumSize = 24;
			buttonList.OnPaintOverride += () =>
			{
				PaintButton( "reorder", ListStyle.List );
				return true;
			};

			buttonGrid.Clicked += () => { CurrentListStyle = ListStyle.Grid; buttonList.Update(); };
			buttonList.Clicked += () => { CurrentListStyle = ListStyle.List; buttonGrid.Update(); };

			tilelistView.ItemAlign = Align.Center;
			tilelistView.ItemSelected = ( item ) =>
			{
				if ( item is TileList data )
				{
					if ( data.isRandom )
					{
						SelectedJsonObject = null;
						SelectedRandomJsonObject = data.ranomObjectList;
					}
					else
					{
						if ( SelectedRandomJsonObject is not null )
						{
							SelectedRandomJsonObject.Clear();
							SelectedRandomJsonObject = null;
							Log.Info( "Clearing Random Object" );
						}
					
						SelectedJsonObject = data.jsonObject;
					}
					UpdatePaintObjectGizmo();
				}
			};

			tilelistView.OnPaintOverride += () => PaintListBackground( tilelistView );
			tilelistView.ItemPaint = PaintBrushItem;

			row.Add( collectionrow );
			row.Add( iconrow );

			row.AddSeparator();
			row.Add( tilelistView );
			row.AddSeparator();
			row.Add( grouprow );

			gridwindowWidget.Layout = row;

			AddOverlay( gridwindowWidget, TextFlag.RightBottom, 0 );
		}
	}
	void UpdateWidgetValues()
	{
		gridwindowWidget.FixedHeight = SceneOverlay.Height;

		window.AlignToParent( TextFlag.CenterTop );

		if ( CurrentListStyle == ListStyle.Grid )
		{
			tilelistView.ItemSize = slider.Value;
		}
		else if ( CurrentListStyle == ListStyle.List )
		{
			tilelistView.ItemSize = new Vector2( 275, slider.Value );
		}
	}
	void MainWindow( SerializedObject so )
	{
		{
			window = new WidgetWindow( SceneOverlay, "Grid Map Controls" );
			window.MaximumWidth = 400;
			window.MinimumWidth = 400;

			var row = Layout.Column();
			paintmode = row.Add( new SegmentedControl() );
			paintmode.AddOption( "Place", "brush" );
			paintmode.AddOption( "Remove", "delete" );
			paintmode.AddOption( "Move", "open_with" );
			paintmode.AddOption( "Copy", "content_copy" );
			paintmode.AddOption( "Decal", "file_download" );
			paintmode.OnSelectedChanged += ( s ) =>
			{
				CurrentPaintMode = Enum.Parse<PaintMode>( s );
				UpdateListViewItems();
				EndGameObjectGizmo();
			};

			var cs = new ControlSheet();

			cs.AddRow( so.GetProperty( "PrefabResourse" ) );

			var row2 = Layout.Row();
			row2.AddSpacingCell( 16 );
			floorcontrolLabel = new Label( $"Floor Level: {floorCount.ToString()}" );
			row2.Add( floorcontrolLabel );

			rotationLabel = new Label( $"Rotation Snap: {rotationSnap}" );
			row2.Add( rotationLabel );

			selectedamount = new Label( $"Selection: {SelectedGroupObjects.Count}" );
			row2.Add( selectedamount );

			var pop = Layout.Row();

			var popbutton = pop.Add( new Button( "Options...", "more_horiz" ) { Clicked = () => { OpenDropdown( window ); } } );
			popbutton.ButtonType = "clear";
			popbutton.OnPaintOverride += () =>
			{
				if ( popbutton.IsUnderMouse || optionsOpened )
				{
					Paint.SetPen( Theme.White.WithAlpha( .75f ) );
					Paint.SetDefaultFont( 8, 450 );
					Paint.DrawText( popbutton.LocalRect + new Vector2( 164, -10 ), "Options" );
					Paint.ClearBrush();
					Paint.ClearPen();
					Paint.SetBrush( Theme.ControlBackground.WithAlpha( .4f ) );
				}

				Paint.SetPen( Theme.White.WithAlpha( .5f ) );
				Paint.SetDefaultFont( 8, 450 );
				Paint.DrawText( popbutton.LocalRect + new Vector2( 164, -10 ), "Options" );
				Paint.ClearBrush();
				Paint.ClearPen();
				Paint.SetBrush( Theme.ControlBackground.WithAlpha( .2f ) );
				//Paint.DrawRect( popbutton.LocalRect );

				return true;
			};
			popbutton.MaximumWidth = 500;
			popbutton.MinimumHeight = 50;

			row.Add( cs );
			row.Add( row2 );
			row.Add( pop );

			//cs.Add( new Button( "Clear", "clear" ) { Clicked = ClearAll } );
			window.Layout = row;
			window.OnPaintOverride += () => PaintControlBackground( window );
			AddOverlay( window, TextFlag.CenterTop, 0 );

		}
	}

	void ThreeDGizmo( SerializedObject so )
	{
		{ 
			var wind = new WidgetWindow( SceneOverlay, "3D Gizmo" );

			var row = Layout.Column();

			gizmowidg = row.Add( new SceneGizmoControl( wind ) );
			row.Add( gizmowidg );
			
			wind.OnPaintOverride += () => PaintGizmoBackground( wind );

			wind.Layout = row;
			AddOverlay( wind, TextFlag.RightTop, new Vector2( 320 ,0) );
		}
	}

	bool optionsOpened = false;
	private SceneGizmoControl gizmowidg;

	void OpenDropdown( Widget window )
	{
		if ( optionsOpened )
		{
			popup.Close();
			optionsOpened = false;
			return;
		}
		optionsOpened = true;
		popup = new WidgetWindow( window );

		popup.WindowTitle = "Options";

		popup.MinimumWidth = window.Width;
		popup.Width = 500;
		popup.Layout = Layout.Column();
		popup.Layout.Margin = 16;
		
		var ps = new PropertySheet( popup );
		ps.AddSectionHeader( "Floors" );
		{
			floorLabel = ps.AddRow( "Current Floor Level:", new Label( floorCount.ToString() ) );
		}
		{
			heightInput = ps.AddRow( "Floor Height:", new LineEdit("Floor Height") );
			heightInput.Bind( "Value" ).From( () => FloorHeight.ToString(), x => { if ( float.TryParse( x, out var f ) ) FloorHeight = f.FloorToInt(); } );
			heightInput.Text = "128";
		}
		{
			var x = ps.AddRow("Floor Level:", new TwoButton() );
			x.button1.Clicked = () => { DoFloors( FloorHeight )(); floorLabel.Text = $"Floor Level: {floorCount.ToString()}"; };
			x.button1.Icon = "arrow_upward";
			x.label1.Text = "Shift + E";
			x.button2.Clicked = () => { DoFloors( -FloorHeight )(); floorLabel.Text = $"Floor Level: {floorCount.ToString()}"; };
			x.button2.Icon = "arrow_downward";
			x.label2.Text = "Shift + Q";
		}

		ps.AddSectionHeader( "Rotation" );
		{
			rotationSnapBox = ps.AddRow( "Rotation Snap :", new ComboBox());
			foreach (var rot in RotationSnaps )
			{
				rotationSnapBox.AddItem( rot.ToString(), null, () => rotationSnap = rot, rotationLabel.Text = $"Rotation Snap: {rotationSnap}" );
			}
			int defaultAngleSnapIndex = Array.IndexOf( RotationSnaps, 90f );
			if ( defaultAngleSnapIndex != -1 )
			{
				rotationSnapBox.CurrentIndex = defaultAngleSnapIndex;
				rotationSnap = 90f;
			}
			var x = ps.AddRow( "Decrease Rotation Snap:", new Label( "Shift + 4" ) );
			var z = ps.AddRow( "Increase Rotation Snap:", new Label( "Shift + 5" ) );
		}
		{
			var z = ps.AddRow( "Rotation Z:", new TwoButton() );
			z.button1.Clicked = () => { DoRotation( true, GroundAxis.Z )(); };
			z.button1.Icon = "arrow_back";
			z.label1.Text = "Shift + 1";
			z.button2.Clicked = () => { DoRotation( false, GroundAxis.Z )(); };
			z.button2.Icon = "arrow_forward";
			z.label2.Text = "Alt + 1";

			var x = ps.AddRow( "Rotation X:", new TwoButton() );
			x.button1.Clicked = () => { DoRotation( true, GroundAxis.X )(); };
			x.button1.Icon = "arrow_back";
			x.label1.Text = "Shift + 2";
			x.button2.Clicked = () => { DoRotation( false, GroundAxis.X )(); };
			x.button2.Icon = "arrow_forward";
			x.label2.Text = "Alt + 2";

			var y = ps.AddRow( "Rotation Y:", new TwoButton() );
			y.button1.Clicked = () => { DoRotation( true, GroundAxis.Y )(); };
			y.button1.Icon = "arrow_back";
			y.label1.Text = "Shift + 3";
			y.button2.Clicked = () => { DoRotation( false, GroundAxis.Y )(); };
			y.button2.Icon = "arrow_forward";
			y.label2.Text = "Alt + 3";
		}
		ps.AddSectionHeader( "Ground Axis" );
		{
			var w = ps.AddRow( "X", new Checkbox( "Shift + C" ) );
			w.Bind( "Value" ).From( () => Axis == GroundAxis.X, x => { if ( x ) Axis = GroundAxis.X; currentaxisLabel.Text = Axis.ToString(); } );
		}
		{
			var w = ps.AddRow( "Y", new Checkbox( "Shift + X" ) );
			w.Bind( "Value" ).From( () => Axis == GroundAxis.Y, x => { if ( x ) Axis = GroundAxis.Y; currentaxisLabel.Text = Axis.ToString(); } );
		}
		{
			var w = ps.AddRow( "Z", new Checkbox( "Shift + Z" ) );
			w.Bind( "Value" ).From( () => Axis == GroundAxis.Z, x => { if ( x ) Axis = GroundAxis.Z; currentaxisLabel.Text = Axis.ToString(); } );
		}
		ps.AddSectionHeader( "Grid" );
		{
			var w = ps.AddRow( "Snap to Grid", new Checkbox( "Shift + G" ) );
			w.Bind( "Value" ).From( () => ShouldSnapToGrid, x => { ShouldSnapToGrid = x; } );
		}
		ps.AddSectionHeader( "Decal" );
		{
			var w = ps.AddRow("Tri Planar", new Checkbox( ) );
			w.Bind( "Value" ).From( () => DecalTriPlanar, x => { DecalTriPlanar = x; } );
		}

		popup.Layout.Add( ps );
		
		AddOverlay( popup, TextFlag.None, window.Position + new Vector2( 0, window.Size.y ) );

		popup.Show();
	}

	private static bool PaintGizmoBackground( Widget widget )
	{
		Paint.ClearPen();
		Paint.ClearBrush();
		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 0f ) );
		Paint.DrawRect( widget.LocalRect );

		return true;
	}
	private static bool PaintControlBackground( Widget widget )
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground.WithAlpha( .0f ) );
		Paint.DrawRect( widget.LocalRect, 12 );

		Paint.ClearBrush();

		Vector2 iconPosition = new Vector2( widget.LocalRect.Left, widget.LocalRect.Top + 32 );
		var bgrect = new Rect( iconPosition.x, iconPosition.y, widget.Width, widget.Height / 1.5f );

		Paint.SetBrush( Theme.ControlBackground.WithAlpha( .5f ) );
		Paint.DrawRect( bgrect, 16 );

		return true;
	}
	private static bool PaintListBackground( Widget widget )
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 1f ) );
		Paint.DrawRect( widget.LocalRect );

		return false;
	}
	private void PaintButton(string icon, ListStyle style)
	{
		if( CurrentListStyle == style )
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.Green.WithAlpha( 0.10f ) );
			Paint.SetPen( Theme.Black.WithAlpha( 0.50f ) );
			Paint.DrawRect( new Rect( 0, 0, 24, 24 ), 3 );
		}

		Paint.ClearPen();
		Paint.SetBrush( Theme.Grey.WithAlpha( 0.10f ) );
		Paint.SetPen( Theme.Black.WithAlpha( 0.50f ) );
		Paint.DrawRect( new Rect( 0, 0, 24, 24 ), 3 );


		Paint.ClearPen();
		Paint.SetPen( Theme.Grey, 2.0f );
		Paint.SetFont( "Poppins", 6, 700 );
		Paint.DrawIcon( new Rect( 0, 0, 24, 24 ), icon, 16 );
	}
	private void PaintBrushItem( VirtualWidget widget )
	{
		var brush = (TileList)widget.Object;

		Paint.Antialiasing = true;
		Paint.TextAntialiasing = true;
	
		if ( brush.jsonObject == SelectedJsonObject && !brush.isRandom )
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.Green.WithAlpha( 0.10f ) );
			Paint.SetPen( Theme.Green.WithAlpha( 0.50f ) );
			Paint.DrawRect( widget.Rect.Grow( 0 ), 3 );
		}

		if ( brush.ranomObjectList == SelectedRandomJsonObject && brush.isRandom )
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.Green.WithAlpha( 0.10f ) );
			Paint.SetPen( Theme.Green.WithAlpha( 0.50f ) );
			Paint.DrawRect( widget.Rect.Grow( 0 ), 3 );
		}

		Paint.ClearPen();
		Paint.SetBrush( Theme.White.WithAlpha( 0.01f ) );
		Paint.SetPen( Theme.White.WithAlpha( 0.05f ) );
		Paint.DrawRect( widget.Rect.Shrink( 2 ), 3 );

		if ( CurrentListStyle == ListStyle.List )
		{
			int iconSize = (int)Math.Min( widget.Rect.Height, widget.Rect.Width / 3 ); // Adjust for icon width ratio
			Vector2 iconPosition = new Vector2(
				widget.Rect.Left + 5, // 5 pixels padding from the left edge
				widget.Rect.Top + (widget.Rect.Height - iconSize) / 2 // Center icon vertically
			);

			var iconRect = new Rect( iconPosition.x, iconPosition.y, iconSize, iconSize );

			Paint.Draw( iconRect, brush.icon );
		}
		else
		{
			Paint.Draw( widget.Rect.Shrink(5), brush.icon );
		}

		var rect = widget.Rect;

		var textRect = rect.Shrink( 2 );

		if ( CurrentListStyle == ListStyle.Grid )
		{
			textRect.Top = textRect.Top + slider.Value * 0.65f;
			textRect.Top = textRect.Top + slider.Value / 10;

			Paint.ClearPen();
			Paint.SetBrush( Theme.Black.WithAlpha( 0.5f ) );
			Paint.DrawRect( textRect, 0.0f );
		}
		else
		{
			textRect.Right = textRect.Right + slider.Value;
		}
		
		Paint.Antialiasing = true;

		Paint.SetPen( Theme.Blue, 2.0f );
		Paint.ClearBrush();
		Paint.SetFont( "Poppins", slider.Value / 8f, 700 );
		Paint.DrawText( textRect, brush.name );

		var offset = -1;

		var iconscale = slider.Value * 0.25f;

		if ( brush.group is null || brush.group == " " )
		{
			Paint.ClearPen();
			Paint.ClearBrush();
			Paint.SetBrush( Theme.Green.WithAlpha( 0.5f ) );
			Paint.ClearPen();
			Paint.DrawRect( new Rect( widget.Rect.Left, widget.Rect.Top, iconscale, iconscale ), 3 );
			Paint.SetPen( Theme.White.WithAlpha( 0.5f ) );
			Paint.DrawIcon( new Rect( widget.Rect.Left, widget.Rect.Top, 1f * iconscale, 1f * iconscale ), "grid_view", 1 * iconscale );
			offset += 1;
		}

		if ( brush.group is not null )
		{
			string input = brush.group.Length > 0 ? brush.group : "";
			int index = input.IndexOf( "+" );

			if ( brush.group.Contains( "+" ) )
			{
				Paint.ClearPen();
				Paint.ClearBrush();
				Paint.SetBrush( Theme.Green.WithAlpha( 0.5f ) );
				Paint.ClearPen();
				Paint.DrawRect( new Rect( widget.Rect.Left, widget.Rect.Top, iconscale, iconscale ), 3 );
				Paint.SetPen( Theme.White.WithAlpha( 0.5f ) );
				string result = input.Substring( index + 1 );
				Paint.DrawIcon( new Rect( widget.Rect.Left, widget.Rect.Top, 1f * iconscale, 1f * iconscale ), result, 1 * iconscale );
				offset += 1;
			}
			else
			{
				Paint.ClearPen();
				Paint.ClearBrush();
				Paint.SetBrush( Theme.Green.WithAlpha( 0.5f ) );
				Paint.ClearPen();
				Paint.DrawRect( new Rect( widget.Rect.Left, widget.Rect.Top, iconscale, iconscale ), 3 );
				Paint.SetPen( Theme.White.WithAlpha( 0.5f ) );
				Paint.DrawIcon( new Rect( widget.Rect.Left, widget.Rect.Top, 1f * iconscale, 1f * iconscale ), "grid_view", 1 * iconscale );
				offset += 1;
			}
		}
		if ( brush.isRandom )
		{
			offset += 1;
			Paint.ClearPen();
			Paint.ClearBrush();
			Paint.SetBrush( Theme.Red.WithAlpha( 0.5f ) );
			Paint.ClearPen();
			Paint.DrawRect( new Rect( widget.Rect.Left + (iconscale * offset ) + (iconscale / 32f), widget.Rect.Top, iconscale, iconscale ), 3 );
			Paint.SetPen( Theme.White.WithAlpha( 0.5f ) );
			Paint.DrawIcon( new Rect( widget.Rect.Left + (iconscale * offset) + (iconscale / 32f), widget.Rect.Top, 1f * iconscale, 1f * iconscale ), "shuffle", 1 * iconscale );
		}
		if( brush.isDecal )
		{
			offset += 1;
			Paint.ClearPen();
			Paint.ClearBrush();
			Paint.SetBrush( Theme.Blue );
			Paint.DrawRect( new Rect( widget.Rect.Left + (iconscale * offset) + (iconscale / 32f), widget.Rect.Top, iconscale, iconscale ), 3 );
			Paint.SetPen( Theme.White );
			Paint.DrawIcon( new Rect( widget.Rect.Left + (iconscale * offset) + (iconscale / 32f), widget.Rect.Top, 1f * iconscale, 1f * iconscale ), "file_download", 1 * iconscale );

		}
		if( brush.isPrefab )
		{
			offset += 1;
			Paint.ClearPen();
			Paint.ClearBrush();
			Paint.SetBrush( Theme.Pink.WithAlpha( 0.5f ) );
			Paint.ClearPen();
			Paint.DrawRect( new Rect( widget.Rect.Left + (iconscale * offset) + (iconscale / 32f), widget.Rect.Top, iconscale, iconscale ), 3 );
			Paint.SetPen( Theme.White.WithAlpha( 0.5f ) );
			Paint.DrawIcon( new Rect( widget.Rect.Left + (iconscale * offset) + (iconscale / 32f), widget.Rect.Top, 1f * iconscale, 1f * iconscale ), "group_work", 1 * iconscale );

		}
	}
}

public class PopUpOptions : WidgetWindow
{
	public PopUpOptions( Widget widget )
	{

	}
	protected override void OnPaint()
	{

	}

}

public class NewCollectionObjectWindow : BaseWindow
{

	Widget container;
	public GridMapTool gridmapToolDockWidget;

	public NewCollectionObjectWindow()
	{
		Size = new Vector2( 350, 150 );
		MinimumSize = Size;
		MaximumSize = Size;
		TranslucentBackground = true;
		NoSystemBackground = true;

		WindowTitle = "Add New Collection Object";
		SetWindowIcon( "rocket_launch" );

		Layout = Layout.Column();
		Layout.Margin = 4;
		Layout.Spacing = 4;

		container = new Widget( this );

		var properties = new PropertySheet( this );

		var nameLabel = new Label.Subtitle( "Add New Collection Object" );
		nameLabel.Margin = 16;

		var nameEdit = properties.AddLineEdit( "Collection Object Name", "" );
		var addbutton = new Button.Primary( "Add Collection", "add_circle" );
		addbutton.MaximumWidth = 100;
		addbutton.Clicked = () =>
		{
			gridmapToolDockWidget.CreateCollection( nameEdit.Text );
			/*
			var gameobject = new GameObject(true, nameEdit.Text );
			gameobject.Transform.Position = Vector3.Zero;
			gameobject.Tags.Add( "gridtile" );
			gameobject.Tags.Add( "collection" );
			*/
			gridmapToolDockWidget.collectionDropDown.AddItem( nameEdit.Text );
			Close();
		};
		Layout.Add( nameLabel );
		Layout.Add( properties );
		Layout.Add( addbutton );
		Layout.Add( container );

		Show();
	}
	public string SetButtonIcon = "settings";

	public Button buttonIcon;
}

public class TwoButton : Widget
{
	public Button button1;
	public Button button2;

	public Label label1;
	public Label label2;

	public TwoButton()
	{
		// Create a horizontal layout for the buttons
		var layout = Layout.Row();
		layout.Spacing = 4;
		// Create the fvar irst button
		button1 = layout.Add( new Button("", this ) );
		button1.ButtonType = "clear";

		label1 = layout.Add( new Label( "", this ) );
		label1.Position = new Vector2( 40, 5 );
		layout.AddSpacingCell( 40 );

		// Create the svar econd button
		button2 = layout.Add( new Button("", this ) );
		button2.ButtonType = "clear";
		button2.Position = new Vector2( 100, 0 );
		layout.Add( button2 );

		label2 = layout.Add( new Label( "", this ) );
		label2.Position = new Vector2( 140, 5 );

		MinimumHeight = 23;
	}
}
public class SceneGizmoControl : Widget
{
	public Rotation CameraRotation;

	private Button freeRotation;

	private Button xAxisButton;
	private Button yAxisButton;
	private Button zAxisButton;

	private Button minxAxisButton;
	private Button minyAxisButton;
	private Button minzAxisButton;

	private const float AxisRadius = 32;
	public SceneGizmoControl( Widget parent = null )
	{
		MaximumWidth = 100;
		MinimumWidth = 100;
		MaximumHeight = 100;
		MinimumHeight = 100;

		freeRotation = new Button( "Free", this );

		xAxisButton = new Button( "X", this );
		yAxisButton = new Button( "Y", this );
		zAxisButton = new Button( "Z", this );

		minxAxisButton = new Button( "-X", this );
		minyAxisButton = new Button( "-Y", this );
		minzAxisButton = new Button( "-Z", this );

		SetupButton( minxAxisButton, Color.Red );
		SetupButton( minyAxisButton, Color.Green );
		SetupButton( minzAxisButton, Color.Blue );

		SetupButton( xAxisButton, Color.Red );
		SetupButton( yAxisButton, Color.Green );
		SetupButton( zAxisButton, Color.Blue );
	}

	private void SetupButton( Button button, Color color )
	{
		button.Size = 20;

		button.Clicked += () => OnAxisButtonClicked( button.Text );
	}

	private void OnAxisButtonClicked( string axis )
	{
		if ( axis == "X" )
		{
			SceneViewportWidget.LastSelected.State.CameraRotation = Rotation.FromYaw( 180 );
		}
		else if ( axis == "Y" )
		{
			SceneViewportWidget.LastSelected.State.CameraRotation = Rotation.FromYaw( 90 );
		}
		else if ( axis == "Z" )
		{
			SceneViewportWidget.LastSelected.State.CameraRotation = Rotation.FromPitch( 90 );
		}
		else if ( axis == "-X" )
		{
			SceneViewportWidget.LastSelected.State.CameraRotation = Rotation.FromYaw( -180 );
		}
		else if ( axis == "-Y" )
		{
			SceneViewportWidget.LastSelected.State.CameraRotation = Rotation.FromYaw( -90 );
		}
		else if ( axis == "-Z" )
		{
			SceneViewportWidget.LastSelected.State.CameraRotation = Rotation.FromPitch( -90 );
		}
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.ClearBrush();
		Paint.ClearPen();

		var center = new Vector2(50, 50);

		var transformedUp = TransformDirection(Vector3.Up, CameraRotation); // Z Axis (Up)
		var transformedForward = TransformDirection(Vector3.Forward, CameraRotation); // X Axis (Forward)
		var transformedLeft = TransformDirection(Vector3.Left, CameraRotation); // Y Axis (Left)

		DrawMinAxisCircle( center, 32, Gizmo.Colors.Pitch, ProjectTo2D(-transformedForward)); // Back Axis
		DrawMinAxisCircle( center, 32, Gizmo.Colors.Yaw, ProjectTo2D(-transformedLeft)); // Right Axis
		DrawMinAxisCircle( center, 32, Gizmo.Colors.Roll, ProjectTo2D(-transformedUp)); // Down Axis
		DrawAxisCircle( center, 32, Gizmo.Colors.Pitch, ProjectTo2D( transformedForward ) ); // Forward Axis
		DrawAxisCircle( center, 32, Gizmo.Colors.Yaw, ProjectTo2D( transformedLeft ) ); // Left Axis
		DrawAxisCircle( center, 32, Gizmo.Colors.Roll, ProjectTo2D( transformedUp ) ); // Up Axis

		UpdateButtonPosition( minxAxisButton, Gizmo.Colors.Pitch.WithAlpha( 0.5f ), ProjectTo2D( -transformedForward ) ); ;
		UpdateButtonPosition( minyAxisButton, Gizmo.Colors.Yaw.WithAlpha( 0.5f ), ProjectTo2D( -transformedLeft ) );
		UpdateButtonPosition( minzAxisButton, Gizmo.Colors.Roll.WithAlpha( 0.5f ), ProjectTo2D( -transformedUp ) );

		UpdateButtonPosition( xAxisButton, Gizmo.Colors.Pitch, ProjectTo2D( transformedForward ) );
		UpdateButtonPosition( yAxisButton, Gizmo.Colors.Yaw ,ProjectTo2D( transformedLeft ) );
		UpdateButtonPosition( zAxisButton, Gizmo.Colors.Roll, ProjectTo2D( transformedUp ) );

		DrawButtonRotationCircle( freeRotation );
	}

	private Vector2 ProjectTo2D( Vector3 vector3D )
	{
		// Orthographic projection onto 2D plane
		return new Vector2( vector3D.x, vector3D.y );
	}

	private void UpdateButtonPosition( Button button,Color color, Vector2 direction )
	{
		var endPoint = new Vector2( 50, 50 ) + direction * AxisRadius;
		button.Position = endPoint - new Vector2( button.Width / 2, button.Height / 2 );

		button.OnPaintOverride = () =>
		{
			var minsbutt = false;
			if ( button.Text.Contains( "-" ) ) 
			{
				minsbutt = true;
			}; 
			DrawButtonAxisCircle( button, 16, color, direction, minsbutt );
			return true;
		};


		if ( freeRotation.IsPressed )
		{
			freeRotation.Cursor = CursorShape.Blank;

			var delta = Application.CursorDelta * 0.1f;
			var angles = SceneViewportWidget.LastSelected.State.CameraRotation.Angles();

			var orbitPosition = SceneViewportWidget.LastSelected.State.CameraPosition + SceneViewportWidget.LastSelected.State.CameraRotation.Forward * 100;

			angles.roll = 0;
			angles.yaw -= delta.x;
			angles.pitch += delta.y;
			angles = angles.Normal;
			angles.pitch = angles.pitch.Clamp( -89, 89 );

			// Apply the rotations to the camera
			SceneViewportWidget.LastSelected.State.CameraRotation = angles.ToRotation();
			
			// Calculate the new camera position based on orbit distance and rotation
			var cameraDirection = SceneViewportWidget.LastSelected.State.CameraRotation.Forward;

			Application.CursorPosition = freeRotation.ScreenPosition + freeRotation.LocalRect.Center;
		}
		else
		{
			freeRotation.Cursor = CursorShape.None;
		}

	}

	private Vector3 TransformDirection( Vector3 direction, Rotation cameraRotation )
	{
		Rotation combinedRotation = cameraRotation;

		return direction * combinedRotation * Rotation.FromAxis(Vector3.Right,90);
	}
	private Vector2 Normalize( Vector2 vector )
	{
		return new Vector2( vector.x, vector.y );
	}
	private void DrawButtonRotationCircle( Button center )
	{
		freeRotation.Size = 100;
		freeRotation.OnPaintOverride = () =>
		{
			Paint.ClearPen();
			Paint.ClearBrush();
			Paint.SetBrush( freeRotation.IsUnderMouse ? Theme.Grey.WithAlpha( 0.35f ) : Theme.Grey.WithAlpha( 0.015f ) );
			Paint.DrawRect( LocalRect, 100 );
			return true;
		};
	}
	private void DrawButtonAxisCircle( Button center, float radius, Color color, Vector2 direction, bool mins )
	{
		Paint.TextAntialiasing = true;
		Paint.Antialiasing = true;
		if ( center.IsUnderMouse )
		{
			Paint.ClearPen();
			Paint.SetPen( Theme.White );
			Paint.SetBrush( color );
			Paint.ClearPen();
			Paint.DrawCircle( center.Size / 2, radius * 1.25f );

			if(mins)
			{
				Paint.SetDefaultFont( 8 , 600 );
				Paint.SetPen( Theme.White );
				Paint.DrawText( center.LocalRect, center.Text );
			}
		}
		if( !mins )
		{
			Paint.ClearPen();
			Paint.SetPen( center.IsUnderMouse ? Theme.White : Theme.Black );
			Paint.SetDefaultFont( 8, 600 );
			Paint.DrawText( center.LocalRect, center.Text );
		}

		Paint.ClearBrush();
		Paint.SetBrush( color.WithAlpha( 0.75f ) );
		Paint.DrawCircle( 32 , 16 );
	}
	
	private void DrawAxisCircle( Vector2 center, float radius, Color color, Vector2 direction )
	{
		Paint.TextAntialiasing = true;
		Paint.Antialiasing = true;
		
		direction = Normalize( direction );

		var endPoint = center + direction * radius;
		
		// Draw the axis line
		Paint.ClearPen();
		Paint.SetBrush( color.Saturate( 0.75f ) );
		Paint.SetPen( color, 2.0f );
		Paint.DrawLine( center, endPoint - direction * 10 );

		Paint.ClearBrush();
		Paint.SetBrush( color.WithAlpha( 0.75f ) );
	
		Paint.DrawCircle( endPoint, 16 );
	}

	private void DrawMinAxisCircle( Vector2 center, float radius, Color color, Vector2 direction )
	{
		Paint.TextAntialiasing = true;
		Paint.Antialiasing = true;
		
		direction = Normalize( direction );

		var endPoint = center + direction * radius;
		// Draw the axis line
		Paint.ClearPen();
		Paint.SetBrush( color );
		Paint.SetPen( color.WithAlpha(0.2f), 2.0f );
		Paint.DrawLine( center, endPoint - direction * 10 );

		Paint.ClearBrush();
		Paint.ClearPen();
		Paint.SetBrush( color.WithAlpha( 0.2f ) );
		Paint.SetPen( color.WithAlpha( 0.5f ), 2.0f );
		Paint.DrawCircle( endPoint, 16 );
	}
}
