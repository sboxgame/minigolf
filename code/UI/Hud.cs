using Sandbox;
using Sandbox.UI;

namespace Minigolf
{
	[UseTemplate]
	public partial class Hud : RootPanel
	{
		public WaitingScreen WaitingScreen { get; set; }

		public override void Tick()
		{
			base.Tick();

			SetClass( "state--playing", Game.Current.State == GameState.Playing );
		}

		[Event( "minigolf.state.changed" )]
		public void OnGameStateChanged( GameState state )
		{
			if ( state != GameState.WaitingForPlayers )
				WaitingScreen?.Delete( false );
		}
	}
}
