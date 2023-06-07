
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Minigolf.UI;

[Library]
public class ButtonHint : Panel
{
	string Button { get; set; }
	Image ImageGlyph { get; init; }

	public ButtonHint()
	{
		ImageGlyph = Add.Image();
	}

    public override void SetProperty(string name, string value)
    {
        base.SetProperty(name, value);

		if ( name == "name" )
			Button = value;
	}

    public override void Tick()
	{
		ImageGlyph.Texture = Input.GetGlyph( Button, InputGlyphSize.Small, GlyphStyle.Light.WithNeutralColorABXY() );
		ImageGlyph.Style.Width = ImageGlyph.Texture.Width;
		ImageGlyph.Style.Height = ImageGlyph.Texture.Height;
	}
}
