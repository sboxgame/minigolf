using System;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minigolf
{
	public partial class OutOfBounds : Panel
	{
		private static List<string> Messages = new List<string>
		{
			"you stupid twat",
			"d'oh!",
			"the lesson is never try"
		};
		
		public static OutOfBounds Current;

		private Label messageLabel;

		public OutOfBounds()
		{
			Current = this;

			StyleSheet.Load("/ui/OutOfBounds.scss");

			Add.Label("OUT OF BOUNDS", "big");
			messageLabel = Add.Label();
		}

		public async Task Show()
		{
			messageLabel.Text = Messages.OrderBy(x => Guid.NewGuid()).First().ToUpper();

			// (GolfHUD.Current as GolfHUD).Fade = true;
			AddClass("show");

			await Task.DelaySeconds(3);

			// (GolfHUD.Current as GolfHUD).Fade = false;
			RemoveClass("show");
		}
	}

}
