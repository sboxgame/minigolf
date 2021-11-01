using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minigolf
{
	public partial class OutOfBounds : Panel
	{
		float DeleteTime;

		public OutOfBounds()
		{
			StyleSheet.Load("/ui/OutOfBounds.scss");

			Add.Label( "Out" );
			Add.Label( "Of" );
			Add.Label( "Bounds" );

			DeleteTime = RealTime.Now + 2.0f;
		}

		public override void Tick()
		{
			if ( RealTime.Now > DeleteTime )
				Delete( false );
		}
	}
}
