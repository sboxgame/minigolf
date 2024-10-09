namespace Facepunch.Minigolf;

public partial class HoleBounds : Component, Component.ITriggerListener
{
	[Property]
	public Hole Hole { get; set; }

	[Property]
	public Collider Collider { get; set; }

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( GameManager.Instance.CurrentHole != Hole )
			return;

		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		GameManager.Instance.UpdateBallInBounds( ball, true );
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		if ( GameManager.Instance.CurrentHole != Hole )
			return;

		var ball = other.GameObject.Root.GetComponentInChildren<Ball>();
		if ( !ball.IsValid() )
			return;

		GameManager.Instance.UpdateBallInBounds( ball, false );
	}
}
