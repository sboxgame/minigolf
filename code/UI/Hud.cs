using Sandbox.UI;

namespace Minigolf
{
	[UseTemplate]
	public partial class Hud : RootPanel
	{
		public override void Tick()
		{
			base.Tick();

			SetClass( "state--playing", Game.Current.State == GameState.Playing );
		}
	}
}
