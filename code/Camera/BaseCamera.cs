using Facepunch.Minigolf.Entities;

namespace Facepunch.Minigolf;

public partial class BaseCamera : Ball.Component
{
	public sealed override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Update();
	}

	public virtual void Update()
	{
		//
	}
}
