namespace Facepunch.Minigolf;

public sealed class MovingPlatform : Component, Component.ExecuteInEditor
{
	[Property, Group( "Configuration" )] public bool AddTagOnStart { get; set; } = true;
	[Property, Group( "Configuration" )] public bool AddConnections { get; set; } = true;
	[Property, Group( "Object" ), InlineEditor] public List<PlatformObject> Platforms { get; set; } = new(); // List of moving platforms
	[Property, Group( "Moving Platform" )] public float Speed { get; set; } = 100.0f;
	[Property, Group( "Loop" )] public bool Loop { get; set; } = true;
	[Property, Group( "Loop" ), ShowIf( "Loop", false )] public bool Flipflop { get; set; } = false;
	[Property, Group( "Loop" )] public float PauseTime { get; set; } = 1.0f;
	[Property, Group( "Debug" )] public bool Debug { get; set; } = false;
	[Property, Group( "PathPoints" )] public List<GameObject> PathPoints { get; set; } = new(); // List of path points

	protected override void OnStart()
	{
		base.OnStart();

		if ( Game.IsPlaying )
			GameObject.Root.Tags.Add( "moving" );

		foreach ( var platform in Platforms )
		{
			// Initialize each platform's starting position based on StartIndex
			platform.CurrentIndex = platform.StartIndex;
			if ( platform.ReverseDirection && Loop )
			{
				// Start moving backward if ReverseDirection is true
				platform.NextIndex = platform.CurrentIndex == 0 ? PathPoints.Count - 1 : platform.CurrentIndex - 1;
				platform.MovingForward = false;
			}
			else
			{
				platform.NextIndex = Loop ? (platform.StartIndex + 1) % PathPoints.Count : Math.Clamp( platform.StartIndex + 1, 0, PathPoints.Count - 1 );
				platform.MovingForward = true;
			}
			platform.Position = PathPoints[platform.CurrentIndex].WorldPosition;
			platform.OffsetTimer = platform.Offset; // Initialize offset timer with the platform's offset
		}

		DoReset();
	}

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( !Gizmo.Settings.GizmosEnabled ) return;

		DrawPathPoints();

		foreach ( var platform in Platforms )
		{
			Gizmo.Draw.Color = Color.Yellow;
			Gizmo.Draw.LineSphere( platform.Position, 5 );

			if ( platform.Object != null )
			{
				Gizmo.Draw.Line( platform.Position, platform.Object.WorldPosition );
				Gizmo.Draw.LineSphere( platform.Object.WorldPosition, 5 );
			}
		}
	}

	void DrawPathPoints()
	{
		Gizmo.Draw.IgnoreDepth = true;
		Gizmo.Transform = new Transform( 0 );
		for ( int i = 0; i < PathPoints.Count; i++ )
		{
			if ( !PathPoints[i].IsValid() ) continue;
			if ( i != PathPoints.Count - 1 || Loop )
			{
				Gizmo.Draw.Color = Flipflop ? Color.Red : Color.Green;
				Gizmo.Draw.Line( PathPoints[i].WorldPosition, PathPoints[(i + 1) % PathPoints.Count].WorldPosition );
			}

			if ( !Gizmo.IsSelected ) return;
			Gizmo.Draw.Color = Color.Cyan;
			Gizmo.Draw.Sprite( PathPoints[i].WorldPosition, 8, "tools/images/common/tag_add.png" );

			Gizmo.Draw.Color = Color.White;
			Gizmo.Draw.Text( i.ToString(), new Transform( PathPoints[i].WorldPosition ), size: 24 );
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( PathPoints.Count < 2 ) return; // Not enough path points to move

		foreach ( var platform in Platforms )
		{
			if ( platform.OffsetTimer > 0 )
			{
				platform.OffsetTimer -= Time.Delta; // Handle offset delay before starting movement
				continue;
			}

			if ( platform.IsPaused )
			{
				platform.PauseTimer -= Time.Delta;
				if ( platform.PauseTimer <= 0 )
				{
					platform.IsPaused = false;
					platform.T = 0.0f; // Reset interpolation
					UpdateIndices( platform ); // Move to next segment
				}
				continue;
			}

			MovePlatform( platform );

			// Update the position of the platform's object
			if ( platform.Object != null )
			{
				if ( Game.IsPlaying )
					platform.Object.WorldPosition = platform.Position;
			}
		}

		if ( AddConnections )
		{
			var lineRenderer = GameObject.GetComponent<LineRenderer>();
			if ( lineRenderer != null )
			{
				lineRenderer.Points ??= new();
				foreach ( var pathPoint in PathPoints )
				{
					if ( lineRenderer.Points.Contains( pathPoint ) )
						continue;
					lineRenderer.Points.Add( pathPoint );
				}

				if ( lineRenderer.Points.Count > PathPoints.Count ) return;

				if ( Loop )
				{
					lineRenderer.Points.Add( PathPoints[0] );

				}
			}
		}
	}

	void MovePlatform( PlatformObject platform )
	{
		Vector3 start = PathPoints[platform.CurrentIndex].WorldPosition;
		Vector3 end = PathPoints[platform.NextIndex].WorldPosition;

		// Interpolate between the current point and the next point
		platform.T += Time.Delta * (Speed / Vector3.DistanceBetween( start, end ));
		platform.Position = Vector3.Lerp( start, end, platform.T );

		if ( platform.T >= 1.0f )
		{
			// Reached the next point
			platform.Position = end;
			platform.IsPaused = true;
			platform.PauseTimer = PauseTime; // Start pause before next move
		}
	}

	void UpdateIndices( PlatformObject platform )
	{
		platform.CurrentIndex = platform.NextIndex;

		if ( Flipflop )
		{
			// Flipflop behavior: reverse direction when reaching the last point
			if ( platform.MovingForward )
			{
				if ( platform.CurrentIndex == PathPoints.Count - 1 )
				{
					platform.MovingForward = false; // Reached the end, reverse direction
				}
				platform.NextIndex = platform.CurrentIndex + 1;
			}
			else
			{
				if ( platform.CurrentIndex == 0 )
				{
					platform.MovingForward = true; // Reached the start, reverse direction again
				}
				platform.NextIndex = platform.CurrentIndex - 1;
			}

			// Clamp indices to stay within bounds
			platform.NextIndex = Math.Clamp( platform.NextIndex, 0, PathPoints.Count - 1 );
		}
		else if ( Loop )
		{
			// Loop behavior with reverse direction support
			if ( platform.ReverseDirection )
			{
				// Reverse direction: move backward and wrap around
				platform.NextIndex = platform.CurrentIndex == 0 ? PathPoints.Count - 1 : platform.CurrentIndex - 1;
			}
			else
			{
				// Normal loop behavior: move forward and wrap around
				platform.NextIndex = (platform.CurrentIndex + 1) % PathPoints.Count;
			}
		}
		else
		{
			// No loop, just move forward until the end
			platform.NextIndex = (int)MathF.Min( platform.NextIndex + 1, PathPoints.Count - 1 );
		}
	}

	[Button( "Reset" ), Group( "Debug" )]
	void DoReset()
	{
		foreach ( var platform in Platforms )
		{
			platform.CurrentIndex = platform.StartIndex;
			if ( platform.ReverseDirection && Loop )
			{
				// Start moving backward if ReverseDirection is true
				platform.NextIndex = platform.CurrentIndex == 0 ? PathPoints.Count - 1 : platform.CurrentIndex - 1;
				platform.MovingForward = false;
			}
			else
			{
				platform.NextIndex = Loop ? (platform.CurrentIndex + 1) % PathPoints.Count : Math.Clamp( platform.CurrentIndex + 1, 0, PathPoints.Count - 1 );
				platform.MovingForward = true;
			}
			platform.Position = PathPoints[platform.CurrentIndex].WorldPosition;
			platform.IsPaused = false;
			platform.PauseTimer = 0.0f;
			platform.OffsetTimer = platform.Offset;
			platform.T = 0.0f;
		}
	}

	[Button( "Update Nodes" ), Group( "PathPoints" )]
	void UpdateNodes()
	{
		// Ensure the list of path points is populated
		PathPoints.Clear();
		GameObject.Children.ForEach( ( child ) =>
		{
			if ( !PathPoints.Contains( child ) )
				PathPoints.Add( child );
			child.Name = "PathPoint " + PathPoints.IndexOf( child );
		} );
		PathPoints.RemoveAll( obj => obj == null );

		DoReset();
	}
}

public class PlatformObject
{
	[Property] public GameObject Object { get; set; } // The platform to be moved
	[Property] public float Offset { get; set; } = 0.0f; // Delay before starting movement
	[Property] public int StartIndex { get; set; } = 0; // Starting path index for the platform
	[Property] public bool ReverseDirection { get; set; } = false; // Reverse direction if looping

	[Hide] public Vector3 Position { get; set; } // Current position of the platform
	[Hide] public int CurrentIndex { get; set; } = 0; // Current path index
	[Hide] public int NextIndex { get; set; } = 1; // Next path index
	[Hide] public float T { get; set; } = 0.0f; // Time variable for lerp
	[Hide] public bool IsPaused { get; set; } = false; // Is the platform paused?
	[Hide] public float PauseTimer { get; set; } = 0.0f; // Timer for pause
	[Hide] public float OffsetTimer { get; set; } = 0.0f; // Timer for initial delay
	[Hide] public bool MovingForward { get; set; } = true; // Direction of movement
}
