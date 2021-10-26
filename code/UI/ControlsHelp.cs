
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

namespace Minigolf
{
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

}
