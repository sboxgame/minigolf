using Sandbox;
using Sandbox.UI;

namespace Facepunch.Minigolf.UI;

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