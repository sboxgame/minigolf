using System.Collections.Generic;
using Editor;
using Sandbox;
using Sandbox.UI;
using Label = Editor.Label;

namespace Facepunch.Minigolf.Editor;

public sealed class AchievementEditor : BaseResourceEditor<CosmeticResource>
{
	private SerializedObject Object { get; set; }

	public AchievementEditor()
	{
		Layout = Layout.Column();
	}

	protected override void Initialize( Asset asset, CosmeticResource resource )
	{
		Layout.Clear( true );

		Object = resource.GetSerialized();

		var sheet = new ControlSheet();
		sheet.AddObject( Object );

		Layout.Add( sheet );

		Object.OnPropertyChanged += NoteChanged;

		var debugWidget = new AchievementListWidget( Resource, Object );

		Layout.Add( debugWidget );
	}
}

file sealed class AchievementListWidget : Widget
{
	public CosmeticResource Resource { get; }
	private SerializedObject Object { get; set; }

	const string PropertyName = "RequiredAchievements";

	public AchievementListWidget( CosmeticResource resource, SerializedObject obj )
	{
		Resource = resource;
		Object = obj;

		var grid = Layout.Grid();

		grid.VerticalSpacing = 4;
		grid.HorizontalSpacing = 8;

		Layout = grid;
		Layout.Margin = new Margin( 16f, 16f, 16f, 16f );

		UpdateGrid();
	}
	
	public void Toggle( string achievementIdent )
	{
		var prop = Object.GetProperty( PropertyName );
		var list = prop
			.GetValue<List<string>>();

		if ( list.Contains( achievementIdent ) )
		{
			list.Remove( achievementIdent );
		}
		else
		{
			list.Add( achievementIdent );
		}

		prop.SetValue( list );
		UpdateGrid();
	}

	public void UpdateGrid()
	{
		var grid = (GridLayout)Layout;

		grid.Clear( true );

		grid.AddCell( 0, 0, new Label.Small( "Required" ), alignment: TextFlag.Center );
		grid.AddCell( 1, 0, new Label.Small( "Title" ), alignment: TextFlag.Center );
		grid.AddCell( 2, 0, new Label.Small( "Name" ), alignment: TextFlag.Center );
		grid.AddCell( 3, 0, new Label.Small( "Gamer Score" ), alignment: TextFlag.Center );

		var row = 1;

		var list = Object.GetProperty( PropertyName )
			.GetValue<List<string>>();

		foreach ( var achievement in Sandbox.Services.Achievements.All )
		{
			var isEnabled = list.Contains( achievement.Name );
			var ico = isEnabled ? "\u2611" : "\u2610";

			var label = grid.AddCell( 0, row, new Label( ico ), alignment: TextFlag.Center );
			label.MouseClick = () =>
			{
				Toggle( achievement.Name );
			};

			grid.AddCell( 1, row, new Label( achievement.Title ), alignment: TextFlag.Center );
			grid.AddCell( 2, row, new Label( achievement.Name ), alignment: TextFlag.Center );
			grid.AddCell( 3, row, new Label( achievement.Score.ToString() ), alignment: TextFlag.Center );
			row++;
		}
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground.Lighten( 0.5f ) );
		Paint.DrawRect( Layout.InnerRect.Grow( 8f ) );
	}
}
