using System.Collections.Generic;
using System.Linq;

public sealed class SplineMesh : Component, Component.ExecuteInEditor
{
	[Property, Group( "Configuration" ), MakeDirty]
	public float Radius { get; set; } = 20f;

	[Property, Group( "Configuration" ), MakeDirty]
	public int RadialSegments { get; set; } = 8;

	[Property, Group( "Configuration" ), MakeDirty]
	public int SplineInterpolation { get; set; } = 10;

	[Property, Group( "Configuration" ), MakeDirty]
	public float SplineTension { get; set; } = 0.5f;

	[Property, Group( "Configuration" ), MakeDirty]
	public float SplineContinuity { get; set; } = 0.5f;

	[Property, Group( "Configuration" ), MakeDirty]
	public float SplineBias { get; set; } = 0.5f;

	[Property, Group( "Visuals" ), MakeDirty]
	public Material ModelMaterial { get; set; }

	[Property, Group( "Points" )]
	public List<GameObject> ControlPoints { get; set; }

	protected override void OnStart()
	{
		Rebuild();
	}

	protected override void OnDirty()
	{
		Rebuild();
	}

	int Hash;
	protected override void OnUpdate()
	{
		// TODO: use gizmos, don't use this hack, it's just temp

		var lastHash = Hash;
		int hash = 0;
		foreach ( var x in ControlPoints )
			hash += x.WorldPosition.GetHashCode();

		if ( lastHash != hash )
			OnDirty();

		Hash = hash;
	}

	/// <summary>
	/// Rebuild the spline mesh, we call this when something changed
	/// </summary>
	private void Rebuild()
	{
		var renderer = GetOrAddComponent<ModelRenderer>();

		// Generate the spline points
		var splinePoints = GenerateSpline();

		// Generate the mesh along the spline
		var mesh = GenerateMesh( splinePoints );
		mesh.Material = ModelMaterial;

		var model = Model.Builder
			.AddMesh( mesh )
			.Create();

		// Set the renderer's model
		renderer.Model = model;
	}

	/// <summary>
	/// Generates a spline based on control points
	/// </summary>
	/// <returns></returns>
	private IEnumerable<Vector3> GenerateSpline()
	{
		var splinePoints = new List<Vector3>();
		if ( ControlPoints.Count < 2 )
		{
			Log.Warning( "Not enough control points to generate spline." );
			return splinePoints;
		}

		return ControlPoints
			.ToList()
			.Select( x => x.WorldPosition )
			.TcbSpline( SplineInterpolation, SplineTension, SplineContinuity, SplineBias );
	}

	private Mesh GenerateMesh( IEnumerable<Vector3> splinePoints )
	{
		var mesh = new Mesh();

		var vertices = new List<Vertex>();
		var indices = new List<int>();

		for ( int i = 0; i < splinePoints.Count(); i++ )
		{
			var forward = GetTangent( i, splinePoints );

			for ( int j = 0; j < RadialSegments; j++ )
			{
				var angle = j * (2f * MathF.PI / RadialSegments);
				var offset = new Vector3( MathF.Cos( angle ), MathF.Sin( angle ), 0 ) * Radius;

				var rotation = Rotation.LookAt( forward );
				offset = rotation * offset;

				vertices.Add( new Vertex
				{
					Position = splinePoints.ElementAt( i ) + offset,
					Normal = offset.Normal,
					TexCoord0 = new Vector2( j / (float)RadialSegments, i / (float)splinePoints.Count() )
				} );
			}
		}

		for ( int i = 0; i < splinePoints.Count() - 1; i++ )
		{
			for ( int j = 0; j < RadialSegments; j++ )
			{
				var nextSegment = (j + 1) % RadialSegments;
				var baseIndex = i * RadialSegments;

				indices.Add( baseIndex + j );
				indices.Add( baseIndex + j + RadialSegments );
				indices.Add( baseIndex + nextSegment );

				indices.Add( baseIndex + nextSegment );
				indices.Add( baseIndex + j + RadialSegments );
				indices.Add( baseIndex + nextSegment + RadialSegments );
			}
		}

		mesh.CreateVertexBuffer<Vertex>( vertices.Count, Vertex.Layout, vertices.ToArray() );
		mesh.CreateIndexBuffer( indices.Count, indices.ToArray() );

		return mesh;
	}

	Vector3 GetTangent( int index, IEnumerable<Vector3> splinePoints )
	{
		if ( index < splinePoints.Count() - 1 )
			return (splinePoints.ElementAt( index + 1 ) - splinePoints.ElementAt( index ) ).Normal;
		else
			return (splinePoints.ElementAt( index ) - splinePoints.ElementAt( index - 1 ) ).Normal;
	}
}
