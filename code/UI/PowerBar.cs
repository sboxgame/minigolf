using Sandbox;
using Sandbox.UI;

using Facepunch.Minigolf.Entities;

namespace Facepunch.Minigolf.UI;

[UseTemplate]
public partial class PowerBar : Panel
{
	Panel Bar { get; set; }
	Label Value { get; set; }
	Label LastPowerAmount { get; set; }
	Panel LastPower { get; set; }

	public override void Tick()
	{
		if ( Local.Pawn is not Ball ball ) return;

		Bar.Style.Width = Length.Percent( ball.ShotPower * 100 );
		Bar.Style.Dirty();
		Bar.SetClass( "is-visible", ball.ShotPower > 0.0f );

		var PowerAmount = ball.ShotPower * 100;
		Value.Text = PowerAmount.ToString("#0") + "%";

		var lastPowerAmount = ball.LastShotPower * 100;
		LastPowerAmount.Text = lastPowerAmount.ToString("#0") + "%";


		if ( ball.LastShotPower > 0.0f )
		{
			LastPower.Style.Left = Length.Percent( ball.LastShotPower * 100 );
			LastPower.Style.Opacity = 1;
			LastPower.Style.Dirty();
		}
	}
}