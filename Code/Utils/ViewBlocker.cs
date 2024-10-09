using Sandbox;

public sealed class ViewBlocker : Component
{
	[RequireComponent]
	public ModelRenderer Renderer { get; set; }

	public bool BlockingView = false;

	protected override void OnUpdate()
	{
		Renderer.Tint = Renderer.Tint.WithAlpha( Renderer.Tint.a.LerpTo( BlockingView ? .4f : 1f, Time.Delta * 6f ) );

		if ( !BlockingView && Renderer.Tint.a.AlmostEqual( 1f, 0.01f ) )
		{
			Renderer.Tint = Renderer.Tint = Renderer.Tint.WithAlpha( 1f );
		}
	}
}
