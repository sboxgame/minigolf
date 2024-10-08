
using System.Diagnostics;

namespace Editor;

public partial class GridMapTool
{
	void CollectionGroupHighLight()
	{
		if ( timeSinceChangedCollection <= 5 )
		{
			var alpha = 1 - (timeSinceChangedCollection * 4);
			using ( Gizmo.Scope( "Collection" ) )
			{
				Gizmo.Draw.Color = Gizmo.Colors.Pitch.WithAlpha( alpha.LerpTo( 1, 0.0f ) );
				Gizmo.Draw.SolidBox( CurrentGameObjectCollection.GetBounds() );

				Gizmo.Draw.Color = Gizmo.Colors.Pitch.WithAlpha( alpha.LerpTo( 1, 0.0f ) );
				Gizmo.Draw.LineBBox( CurrentGameObjectCollection.GetBounds() );
			}
		}
	}

	public void GroundGizmo( Ray cursorRay )
	{
		projectedPoint = ProjectRayOntoGroundPlane( cursorRay.Position, cursorRay.Forward, floors );

		if ( CurrentPaintMode == PaintMode.Decal ) return;

		using ( Gizmo.Scope( "Ground" ) )
		{
			Gizmo.Draw.LineThickness = 2;

			Gizmo.Draw.Color = Gizmo.Colors.Roll.WithAlpha(0.65f);
			Gizmo.Draw.Line( Vector3.Up , Vector3.Up * 16384 );
			Gizmo.Draw.Line( Vector3.Down , Vector3.Down * 16384 );

			Gizmo.Draw.Color = Gizmo.Colors.Yaw.WithAlpha( 0.65f );
			Gizmo.Draw.Line( Vector3.Left, Vector3.Left * 16384 );
			Gizmo.Draw.Line( Vector3.Right, Vector3.Right * 16384 );

			Gizmo.Draw.Color = Gizmo.Colors.Pitch.WithAlpha( 0.65f );
			Gizmo.Draw.Line( Vector3.Backward, Vector3.Backward * 16384 );
			Gizmo.Draw.Line( Vector3.Forward, Vector3.Forward * 16384 );
		}

		if ( projectedPoint.Hit )
		{
			// Snap the projected point to the grid and adjust for floor height
			var snappedPosition = projectedPoint.EndPosition;

			Vector3 normal = Vector3.Up; // Default for Z-axis
			switch ( Axis )
			{
				case GroundAxis.X:
					normal = Vector3.Forward; // Normal for X-axis
					break;
				case GroundAxis.Y:
					normal = Vector3.Left; // Normal for Y-axis
					break;
					// Z-axis is already set as default
			}

			using ( Gizmo.Scope( "Ground") )
			{
				var axiscolor = Gizmo.Colors.Up;
				switch ( Axis )
				{
					case GroundAxis.X:
						axiscolor = Gizmo.Colors.Forward;
						break;
					case GroundAxis.Y:
						axiscolor = Gizmo.Colors.Left;
						break;
				}

				Gizmo.Draw.Color = axiscolor.WithAlpha(0.25f);

				// Calculate the half size of the grid spacing
				float halfGridSize = Gizmo.Settings.GridSpacing / 2.0f;

				// Create a flat box based on the grid spacing
				Vector3 minCorner = snappedPosition - new Vector3( halfGridSize, halfGridSize, 0 );
				switch ( Axis )
				{
					case GroundAxis.X:
						minCorner = snappedPosition - new Vector3( 0, halfGridSize, halfGridSize );
						break;
					case GroundAxis.Y:
						minCorner = snappedPosition - new Vector3( halfGridSize, 0, halfGridSize );
						break;
				}
				Vector3 maxCorner = snappedPosition + new Vector3( halfGridSize, halfGridSize, 0 );
				switch ( Axis )
				{
					case GroundAxis.X:
						maxCorner = snappedPosition + new Vector3( 0, halfGridSize, halfGridSize );
						break;
					case GroundAxis.Y:
						maxCorner = snappedPosition + new Vector3( halfGridSize, 0, halfGridSize );
						break;
				}
				var bbox = new BBox( minCorner, maxCorner );
				Gizmo.Draw.LineThickness = 6;
				Gizmo.Draw.Color = axiscolor.WithAlpha( 1f );
				Gizmo.Draw.LineBBox( bbox );
				Gizmo.Transform = new Transform( 0, Rotation.FromAxis( Vector3.Up, 45 ) );
			}
		}
	}

	void UpdatePaintObjectGizmo()
	{
		EndGameObjectGizmo();
	}

	public void PaintGizmos( SceneTraceResult tr )
	{
		// Do gizmos and stuff
		var cursorRay = Gizmo.CurrentRay;

		/*
		if ( !boxtr.Hit )
		{
			Vector3 rayOrigin = cursorRay.Position;
			Vector3 rayDirection = cursorRay.Forward;

			boxtr = ProjectRayOntoGroundPlane( rayOrigin, rayDirection, 0 );
		}
		*/
		if ( CurrentPaintMode == PaintMode.Place )
		{
			PlaceGameObjectGizmo( tr, cursorRay );		
		}

		if ( CurrentPaintMode != PaintMode.Place )
		{
		//	EndGameObjectGizmo();
		}

		if(CurrentPaintMode == PaintMode.Decal)
		{
			PlaceDecalObjectGizmo();
		}

		if ( CurrentPaintMode == PaintMode.Remove )
		{
			using ( Gizmo.Scope( "preview" ) )
			{
				var tr2 = Scene.Trace.Ray( cursorRay, 5000 )
						.UseRenderMeshes( true )
						.UsePhysicsWorld( false )
						.WithTag( "gridtile" )
						.Run();

				if ( tr2.Hit )
				{
					Gizmo.Draw.Color = Color.Red.WithAlpha( 0.5f );
					Gizmo.Draw.LineBBox( tr2.GameObject.GetBounds() );
					Gizmo.Draw.SolidBox( tr2.GameObject.GetBounds() );
				}
			}
		}
		else if ( CurrentPaintMode == PaintMode.Move )
		{
			using ( Gizmo.Scope( "preview" ) )
			{
				var tr2 = Scene.Trace.Ray( cursorRay, 5000 )
						.UseRenderMeshes( true )
						.UsePhysicsWorld( false )
						.WithTag( "gridtile" )
						.Run();
				
				if ( tr2.Hit && SelectedObject is null )
				{
					Gizmo.Draw.Color = Theme.Blue.WithAlpha( 0.5f );
					Gizmo.Draw.LineBBox( tr2.GameObject.GetBounds() );
					Gizmo.Draw.SolidBox( tr2.GameObject.GetBounds() );
				}
				else if ( SelectedObject is not null )
				{
					Gizmo.Draw.Color = Theme.Blue.WithAlpha( 0.5f );
					Gizmo.Draw.LineBBox( SelectedObject.GetBounds() );
					Gizmo.Draw.SolidBox( SelectedObject.GetBounds() );
				}
			}
		}
		else if ( CurrentPaintMode == PaintMode.Copy )
		{
			using ( Gizmo.Scope( "preview" ) )
			{
				var tr2 = Scene.Trace.Ray( cursorRay, 5000 )
						.UseRenderMeshes( true )
						.UsePhysicsWorld( false )
						.WithTag( "gridtile" )
						.Run();

				if ( CopyObject is not null )
				{
					Gizmo.Transform = new Transform( GetGizmoPosition( tr2, cursorRay ), Rotation.FromPitch( -90 ) * rotation);
					CopyGameObjectGizmo( tr, cursorRay );
					Gizmo.Draw.Color = Color.Yellow.WithAlpha( 0.15f );
					Gizmo.Draw.LineBBox( GizmoGameObject.GetBounds() );
					Gizmo.Draw.SolidBox( GizmoGameObject.GetBounds() );
				}
				else if ( tr2.Hit && CopyObject is null )
				{
					Gizmo.Draw.Color = Theme.Yellow.WithAlpha( 0.5f );
					Gizmo.Draw.LineBBox( tr2.GameObject.GetBounds() );
					Gizmo.Draw.SolidBox( tr2.GameObject.GetBounds() );
				}
			}
		}
	}
	void CopyGameObjectGizmo( SceneTraceResult trace, Ray cursorRay )
	{
		if ( CopyObject is null ) return;

		if ( GizmoGameObject is null )
		{
			/*
			SceneUtility.MakeGameObjectsUnique( CopyObject );
			//Log.Info( SelectedGameObject.Components.Count );
			GizmoGameObject = new GameObject();
			GizmoGameObject.Deserialize(CopyObject);
			*/
			GizmoGameObject = CopyObject.Clone();
			GizmoGameObject.MakeNameUnique();
			GizmoGameObject.Flags = GameObjectFlags.NotSaved | GameObjectFlags.Hidden;
			GizmoGameObject.Tags.Add( "isgizmoobject" );

		}

		if ( GizmoGameObject is not null )
		{
			GizmoGameObject.Transform.Position = GetGizmoPosition( trace, cursorRay );
			GizmoGameObject.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;
			Log.Info( "GizmoGameObject is not null" );
		}
	}
	List<DuplicatedItems> gizmoDuplicate = new List<DuplicatedItems>();
	GameObject GizmoDuplicateObject { get; set; }
	public void HandleDuplicate( SceneTraceResult trace, Ray cursorRay )
	{
		projectedPoint = ProjectRayOntoGroundPlane( cursorRay.Position, cursorRay.Forward, floors );

		if ( GizmoDuplicateObject is null )
		{
			GizmoDuplicateObject = new GameObject();
			GizmoDuplicateObject.MakeNameUnique();
			GizmoDuplicateObject.Flags = GameObjectFlags.NotSaved | GameObjectFlags.Hidden;
			GizmoDuplicateObject.Tags.Add( "isgizmoobject" );
			GizmoDuplicateObject.Transform.Position = projectedPoint.EndPosition;
		}
		
		foreach ( var obj in DuplicateObjectCollection )
		{
			if ( !gizmoDuplicate.Contains(obj) )
			{
				var dobj = obj.gameObject.Clone();
				dobj.Parent = GizmoDuplicateObject;
				dobj.Transform.Position = obj.position;
				dobj.Transform.Rotation = obj.rotation;
				dobj.MakeNameUnique();
				dobj.Flags = GameObjectFlags.NotSaved | GameObjectFlags.Hidden;
				dobj.Tags.Add( "isgizmoobject" );
				
				gizmoDuplicate.Add( obj ); 
				Log.Info( $"Duplicated:{obj.gameObject.Name}" );
			}
		}
		
		if ( GizmoDuplicateObject is not null )
		{
			GizmoDuplicateObject.Transform.Position = GetGizmoPosition( trace, cursorRay );
			GizmoDuplicateObject.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;

			using ( Gizmo.Scope( "selection_box" ) )
			{
				var rect = GizmoDuplicateObject.GetBounds();
				Gizmo.Draw.Color = Color.Blue.WithAlpha( 0.25f );
				Gizmo.Draw.SolidBox( rect );
				Gizmo.Draw.Color = Color.Blue;
				Gizmo.Draw.LineBBox( rect );
			}
		}
	}

	void EndDuplicateGizmo()
	{
		if ( GizmoDuplicateObject is not null && DuplicateObjectCollection is not null )
		{
			GizmoDuplicateObject.Destroy();
			GizmoDuplicateObject = null;
			gizmoDuplicate.Clear();
			DuplicateObjectCollection.Clear();

			Log.Info( "End Duplicate" );
		}
	}
	
	void PlaceGameObjectGizmo( SceneTraceResult trace, Ray cursorRay )
	{
		if ( SelectedJsonObject is not null )
		{
			if ( GizmoGameObject is null )
			{

					//Log.Info( SelectedGameObject.Components.Count );
					GizmoGameObject = new GameObject( true, "GizmoObject" );
					PrefabUtility.MakeGameObjectsUnique( SelectedJsonObject );
					GizmoGameObject.Deserialize( SelectedJsonObject );
					GizmoGameObject.MakeNameUnique();
					GizmoGameObject.Tags.RemoveAll();
					GizmoGameObject.Tags.Add( "isgizmoobject" );
					GizmoGameObject.Name = "GizmoObject";
					GizmoGameObject.Flags |= GameObjectFlags.NotSaved | GameObjectFlags.Hidden;

					Log.Info( "GizmoGameObject is null" );
			}
		}
		
		if(SelectedRandomJsonObject is not null)
		{
			if ( GizmoGameObject is null )
			{
				if ( SelectedRandomJsonObject.Count != 0 )
				{

					//Log.Info( SelectedGameObject.Components.Count );
					GizmoGameObject = new GameObject( true, "GizmoObject" );
					PrefabUtility.MakeGameObjectsUnique( SelectedRandomJsonObject.FirstOrDefault() );
					GizmoGameObject.Deserialize( SelectedRandomJsonObject.FirstOrDefault() );
					GizmoGameObject.MakeNameUnique();
					GizmoGameObject.Tags.RemoveAll();
					GizmoGameObject.Tags.Add( "isgizmoobject" );
					GizmoGameObject.Name = "GizmoObject";
					GizmoGameObject.Flags |= GameObjectFlags.NotSaved | GameObjectFlags.Hidden;

					Log.Info( "GizmoGameObject is not null" );
				}
			}
		}

		if ( GizmoGameObject is not null )
		{
			GizmoGameObject.Transform.Position = GetGizmoPosition( trace, cursorRay );
			GizmoGameObject.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;
		}

		//Log.Info( SelectedRandomJsonObject.Count );
	}

	void EndGameObjectGizmo()
	{
		if ( GizmoGameObject is not null )
		{
			GizmoGameObject.Destroy();
			GizmoGameObject = null;
		}
	}

	void PlaceDecalObjectGizmo( )
	{
		var cursorRay = Gizmo.CurrentRay;

		var trdecal = SceneEditorSession.Active.Scene.Trace
			.Ray( cursorRay, 5000 )
				.UseRenderMeshes( true )
				.UsePhysicsWorld( true )
				.Run();

		if ( SelectedJsonObject is not null )
		{
			if ( GizmoGameObject is null )
			{
				//Log.Info( SelectedGameObject.Components.Count );
				GizmoGameObject = new GameObject( true, "GizmoObject" );
				PrefabUtility.MakeGameObjectsUnique( SelectedJsonObject );
				GizmoGameObject.Deserialize( SelectedJsonObject );

				GizmoGameObject.MakeNameUnique();
				GizmoGameObject.Tags.RemoveAll();
				GizmoGameObject.Tags.Add( "isgizmoobject" );
				GizmoGameObject.Name = "GizmoObject";
				GizmoGameObject.Flags |= GameObjectFlags.NotSaved | GameObjectFlags.Hidden;

				Log.Info( "GizmoGameObject is null" );
			}
		}

		if ( GizmoGameObject is not null )
		{
			GizmoGameObject.Transform.Position = trdecal.HitPosition.SnapToGrid( Gizmo.Settings.GridSpacing / 2 ) + trdecal.Normal * 10.0f;
			GizmoGameObject.Transform.Rotation = Rotation.LookAt( trdecal.Normal ) * rotation;

			var decal = GizmoGameObject.Components.Get<DecalRenderer>( FindMode.EnabledInSelf );
			if ( decal is not null )
			{
				decal.Size = new Vector3( decalX, decalY, decalZ );
				decal.TriPlanar = DecalTriPlanar;
			}
		}
	}

	private Vector3 GetGizmoPosition( SceneTraceResult trace, Ray cursorRay )
	{
		trace = ProjectRayOntoGroundPlane( cursorRay.Position, cursorRay.Forward, floors );

		if ( trace.Hit )
		{
			var snappedPosition = trace.EndPosition;

			return snappedPosition;
		}

		return Vector3.Zero;
	}

}
