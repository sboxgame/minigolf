using Sandbox;
using Sandbox.UI;

namespace Minigolf
{
	public partial class PowerBar : Panel
	{
		Panel Bar { get; set; }
		Panel LastPower { get; set; }

		public PowerBar()
		{
			SetTemplate( "/UI/PowerBar.html" );
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Ball ball ) return;

			Bar.Style.Width = Length.Percent( ball.ShotPower * 100 );
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
