namespace Facepunch.Minigolf;

public partial class HoleBounds : Component, Component.ITriggerListener
{
	[Property]
	public Hole Hole { get; set; }

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

	protected override void DrawGizmos()
	{
		Gizmo.Transform = new Transform();

		var col = Color.Green;
		foreach ( var collider in GetComponentsInChildren<Collider>() )
		{
			Gizmo.Draw.Color = col;
			Gizmo.Draw.LineBBox( collider.KeyframeBody.GetBounds() );

			Gizmo.Draw.Color = col.WithAlpha( 0.35f );
			Gizmo.Draw.SolidBox( collider.KeyframeBody.GetBounds() );
		}
	}
}
