using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Minigolf
{
	public partial class Hud : RootPanel
	{
		public Hud()
		{
			SetTemplate( "/UI/Hud.html" );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "state--playing", Game.Current.State == GameState.Playing );
		}
	}
}
