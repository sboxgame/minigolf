namespace Facepunch.Minigolf.UI;

[UseTemplate]
public partial class GolfRootPanel : RootPanel
{
	public WaitingScreen WaitingScreen { get; set; }

	public override void Tick()
	{
		base.Tick();

		SetClass( "state--playing", MinigolfGame.Current.State == GameState.Playing );
	}

	protected override void PostTemplateApplied()
	{
		if ( MinigolfGame.Current.State != GameState.WaitingForPlayers )
			WaitingScreen?.Delete( false );
	}

	[MinigolfEvent.StateChange]
	public void OnGameStateChanged( GameState state )
	{
		if ( state != GameState.WaitingForPlayers )
			WaitingScreen?.Delete( false );
	}
}
