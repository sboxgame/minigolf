using Sandbox;
using Sandbox.UI;

namespace Facepunch.Minigolf.UI;

[UseTemplate]
public partial class GolfRootPanel : RootPanel
{
	public WaitingScreen WaitingScreen { get; set; }

	public Panel Freecam { get; set; }
	public string FreecamTime => $"00:{ Game.Current.FreeCamTimeLeft.CeilToInt().ToString( "D2" ) }";

	public override void Tick()
	{
		base.Tick();

		SetClass( "state--playing", Game.Current.State == GameState.Playing );

		Freecam?.SetClass( "freecam--active", Game.Current.FreeCamera != null );
	}

	protected override void PostTemplateApplied()
	{
		if ( Game.Current.State != GameState.WaitingForPlayers )
			WaitingScreen?.Delete( false );
	}

	[Event( "minigolf.state.changed" )]
	public void OnGameStateChanged( GameState state )
	{
		if ( state != GameState.WaitingForPlayers )
			WaitingScreen?.Delete( false );
	}
}