
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

namespace Facepunch.Minigolf.UI;

[Library]
public class ButtonHint : Panel
{
	[Property]
	public string Name { get; set; }

	public InputButton InputButton { get; set; }

	public Label LabelAction { get; set; }
	public Image ImageGlyph { get; set; }

	public ButtonHint()
	{
		ImageGlyph = Add.Image();
		LabelAction = Add.Label();
	}

	public override void Tick()
	{
		InputButton = System.Enum.Parse<InputButton>(Name, true);

		LabelAction.SetText(Name);
		ImageGlyph.Texture = Input.GetGlyph(InputButton, InputGlyphSize.Small);

		ImageGlyph.Style.Width = ImageGlyph.Texture.Width;
		ImageGlyph.Style.Height = ImageGlyph.Texture.Height;
	}
}

public partial class ControlsHelp : Panel
{
	public ControlsHelp()
	{
		StyleSheet.Load( "/UI/ControlsHelp.scss" );

		AddButton( "Free Cam", "iv_view" );
		AddButton( "Reset", "iv_reload" );
		AddButton( "Stroke", "iv_attack" );
		AddButton( "Scoreboard", "iv_score" );
	}

	protected void AddButton( string name, string bind )
	{
		var key = Input.GetKeyWithBinding( bind ) ?? "Unbound";

		var row = Add.Panel( "row" );
		row.Add.Label( name, "name" );
		row.Add.Label( key, "key" );
	}
}
