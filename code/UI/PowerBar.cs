using Sandbox;
using Sandbox.UI;

namespace Minigolf
{
	[UseTemplate]
	public partial class PowerBar : Panel
	{
		Panel Bar { get; set; }
		Panel LastPower { get; set; }

		public override void Tick()
		{
			if ( Local.Pawn is not Ball ball ) return;

			var percentPower = Length.Percent( ball.ShotPower * 100 );
			Bar.Style.Width = percentPower?.Value > 1 ? percentPower : 0;
			Bar.Style.Dirty();

			if ( ball.LastShotPower > 0.0f )
			{
				LastPower.Style.Left = Length.Percent( ball.LastShotPower * 100 );
				LastPower.Style.Opacity = 1;
				LastPower.Style.Dirty();
			}
		}
	}

}
