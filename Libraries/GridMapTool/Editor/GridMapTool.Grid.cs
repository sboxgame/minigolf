using Sandbox.Engine;
using Editor;
using Editor.MapEditor;
using Editor.MapDoc;

namespace Editor;

public partial class GridMapTool
{
	static Material GridMaterial = Material.Load( "materials/grid/grid_material.vmat" );

	List<Vertex> gridVertices = new();
	
	private SceneModel so;
	public void Grid( Vector2 size, Vector3 vector3, float spacing = 32.0f, float opacity = 1.0f, float minorLineWidth = 0.01f, float majorLineWidth = 0.02f )
	{
		if ( so is null )
		{
			//This is a hacky to get the grid to work.
			so = new SceneModel( Scene.SceneWorld, "models/grid/grid.vmdl", new Transform( new Vector3( 0, 0, 0 ) ) );
			so.SetMaterialOverride( GridMaterial );
			so.RenderLayer = SceneRenderLayer.OverlayWithDepth;
			so.Bounds = new BBox( new Vector3( -size.x / 2, -size.y / 2, 0 ), new Vector3( size.x / 2, size.y / 2, 0 ) );
			so.Rotation = Rotation.FromAxis( vector3, 90 );
			switch ( Axis )
			{
				case GroundAxis.Z:
					so.Position = new Vector3( 0, 0, floors );
					break;
				case GroundAxis.X:
					so.Position = new Vector3( floors, 0, 0 );
					break;
				case GroundAxis.Y:
					so.Position = new Vector3( 0, floors, 0 );
					break;
			}
		}
		so.Attributes.Set( "GridScale", spacing );
		so.Attributes.Set( "MinorLineWidth", 0.0125f );
		so.Attributes.Set( "MajorLineWidth", 0.025f );
		so.Attributes.Set( "AxisLineWidth", 0.03f  );
		so.Attributes.Set( "MinorLineColor", new Vector4( 1, 0.5f, 0, 0.75f ) );
		so.Attributes.Set( "MajorLineColor", new Vector4( 1, 0.5f, 0, 1f ) );
		so.Attributes.Set( "XAxisColor", new Vector4( 1, 0.5f, 0, 0.0f ) );
		so.Attributes.Set( "YAxisColor", new Vector4( 1, 0.5f, 0, 0.0f ) );
		so.Attributes.Set( "ZAxisColor", new Vector4( 1, 0.5f, 0, 0.0f ) );
		so.Attributes.Set( "CenterColor", new Vector4( 1, 0.5f, 0, 1.0f ) );
		so.Attributes.Set( "MajorGridDivisions", 16.0f );
	}
}
