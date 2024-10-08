using System.Collections.Generic;

/// <summary>
/// This could be way better. I'm pretty sure I don't need to build a model every frame
/// </summary>
public sealed class BallArrow : Component
{
	[Property]
	public ModelRenderer Renderer { get; set; }

	[Property]
	public Material Material { get; set; }

	public void Build( Vector3 direction, float power = 100 )
	{
		var startPos = 0;
		var endPos = 0 + direction * power * 100;
		var size = Vector3.Cross( direction, Vector3.Up ) * 2f;

		List<Vertex> vertex = new();

		var indices = new List<int>();

		// Line
		Vertex a = new( startPos - size, Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );
		Vertex b = new( startPos + size, Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
		Vertex c = new( endPos + size, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
		Vertex d = new( endPos - size, Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );

		vertex.Add( a );
		vertex.Add( b );
		vertex.Add( c );
		vertex.Add( d );

		indices.Add( vertex.Count - 4 );
		indices.Add( vertex.Count - 3 );
		indices.Add( vertex.Count - 2 );

		indices.Add( vertex.Count - 2 );
		indices.Add( vertex.Count - 1 );
		indices.Add( vertex.Count - 4 );

		// Add the arrow tip
		Vertex e = new( endPos + size * 1.75f, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
		Vertex f = new( endPos - size * 1.75f, Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );
		Vertex g = new( endPos + direction * 8, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );

		vertex.Add( e );
		vertex.Add( f );
		vertex.Add( g );

		indices.Add( vertex.Count - 1 );
		indices.Add( vertex.Count - 2 );
		indices.Add( vertex.Count - 3 );

		var mesh = new Mesh( Material );
		mesh.CreateVertexBuffer( vertex.Count, Vertex.Layout, vertex );
		mesh.CreateIndexBuffer( indices.Count, indices );

		var model = Model.Builder
			.AddMesh( mesh )
			.Create();

		Renderer.Model = model;
	}
}
