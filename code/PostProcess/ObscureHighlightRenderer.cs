using Facepunch.Minigolf.Entities;

namespace Facepunch.Minigolf;

/// <summary>
/// Draws a highlight outline effect on obscured balls.
/// </summary>
[SceneCamera.AutomaticRenderHook]
internal class ObscureHighlightRenderer : RenderHook
{
	public override void OnStage( SceneCamera target, Stage renderStage )
	{
		if ( renderStage == Stage.AfterTransparent )
		{
			RenderEffect();
		}
	}

	public static void RenderEffect()
	{
		if ( Sandbox.Game.LocalPawn is not Ball ball ) return;

		var shapeMat = Material.FromShader( "HighlightObject.vfx" );
		var screenMat = Material.FromShader( "HighlightPostProcess.vfx" );
		
		Graphics.GrabDepthTexture( "DepthTexture" );

		using var rt = RenderTarget.GetTemporary();

		//
		// we need to have a depth buffer here too
		//
		Graphics.RenderTarget = rt;

		Graphics.Clear( Color.Black, true, true, false );
		Graphics.Render( ball.SceneObject, material: shapeMat );

		Graphics.RenderTarget = null;

		RenderAttributes materialAttributes = new RenderAttributes();
		materialAttributes.Set( "ColorBuffer", rt.ColorTarget );
		materialAttributes.Set( "LineColor", Color.Transparent );
		materialAttributes.Set( "OccludeColor", Color.White.WithAlpha( 0.8f ) );
		materialAttributes.Set( "LineWidth", 0.25f );
		Graphics.Blit( screenMat, materialAttributes );
	}
}
