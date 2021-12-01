using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minigolf
{
	[UseTemplate]
	public partial class OutOfBounds : Panel
	{
		float DeleteTime;

		public OutOfBounds()
		{

	

			DeleteTime = RealTime.Now + 2.0f;
		}

		public override void Tick()
		{
			if ( RealTime.Now > DeleteTime )
				Delete( false );
		}
	}
}
