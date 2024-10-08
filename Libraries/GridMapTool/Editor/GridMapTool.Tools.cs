using Sandbox;
using System.Diagnostics;
using System.Drawing;

namespace Editor;

public partial class GridMapTool
{
	private SceneTraceResult projectedPoint;
	public int FloorHeight = 128;

	public SceneTraceResult CursorRay (Ray cursorRay )
	{
		var tr = Scene.Trace.Ray( cursorRay, 5000 )
		.UseRenderMeshes( true )
		.UsePhysicsWorld( false )
		.WithTag( "gridtile" )
		.Run();

		return tr;
	}
	public void HandlePlacement( SceneTraceResult tr, Ray cursorRay )
	{
		//if ( SelectedJsonObject is null ) return;
		using var scope = SceneEditorSession.Scope();
		
		projectedPoint = ProjectRayOntoGroundPlane( cursorRay.Position, cursorRay.Forward, floors );
		
		if ( projectedPoint.Hit )
		{
			// Snap the projected point to the grid and adjust for floor height
			var snappedPosition = projectedPoint.EndPosition;

			if ( SelectedRandomJsonObject is not null && SelectedRandomJsonObject.Count != 0 )
			{
				// Create an instance of the Random class
				Random random = new Random();

				// Get a random index
				int randomIndex = random.Next( SelectedRandomJsonObject.Count );

				// Select a random tile from the list
				var randomTileJson = SelectedRandomJsonObject[randomIndex];

				// Instantiate the game object
				var go = new GameObject( true, "GridTile" );
				PrefabUtility.MakeGameObjectsUnique( randomTileJson );
				go.Deserialize( randomTileJson );
				go.Parent = CurrentGameObjectCollection;
				go.Transform.Position = snappedPosition;
				go.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;
				go.Tags.Remove( "group" );
				go.Tags.Add( "gridtile" );

				go.EditLog( "Grid Placed", go );
			}

			if ( SelectedJsonObject is not null )
			{
				var go = new GameObject( true, "GridTile" );
				PrefabUtility.MakeGameObjectsUnique( SelectedJsonObject );
				go.Deserialize( SelectedJsonObject );
				go.Parent = CurrentGameObjectCollection;
				go.Transform.Position = snappedPosition;
				go.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;
				go.Tags.Remove( "group" );
				go.Tags.Add( "gridtile" );

				go.EditLog( "Grid Placed", go );
			}
		}
	}

	public void HandleDecalPlace()
	{
		var cursorRay = Gizmo.CurrentRay;

		var trdecal = SceneEditorSession.Active.Scene.Trace
			.Ray( cursorRay, 5000 )
				.UseRenderMeshes( true )
				.UsePhysicsWorld( true )
				.Run();

		if(trdecal.Hit )
		{
			if ( SelectedJsonObject is not null )
			{
				var go = new GameObject( true, "GridTile" );
				PrefabUtility.MakeGameObjectsUnique( SelectedJsonObject );
				go.Deserialize( SelectedJsonObject );
				go.Parent = CurrentGameObjectCollection;
				go.Transform.Position = GizmoGameObject.Transform.Position;
				go.Transform.Rotation = GizmoGameObject.Transform.Rotation;
				go.Tags.Remove( "group" );
				go.Tags.Add( "gridtile" );
				var addition = go.Components.Get<DecalRenderer>();
				addition.TriPlanar = DecalTriPlanar;
				addition.Size = new Vector3( decalX, decalY, decalZ );

				go.EditLog( "Grid Placed", go );
			}
		}
	}

	public void HandlePlaceDuplicatedGroup( SceneTraceResult tr, Ray cursorRay )
	{
		projectedPoint = ProjectRayOntoGroundPlane( cursorRay.Position, cursorRay.Forward, floors );

		foreach ( var obj in GizmoDuplicateObject.Children )
		{

			obj.Flags = GameObjectFlags.None;
			obj.Tags.Remove( "isgizmoobject" );

			using var scope = SceneEditorSession.Scope();
			
			var options = new GameObject.SerializeOptions();
			var selection = obj;
			var json = selection.Serialize( options );

			SceneUtility.MakeGameObjectsUnique( json );
			var go = SceneEditorSession.Active.Scene.CreateObject();
		
			go.Deserialize( json );
			go.MakeNameUnique();
			go.Parent = CurrentGameObjectCollection;
			go.Transform.Position = obj.Transform.Position;
			go.Transform.Rotation = obj.Transform.Rotation;

			go.Tags.Add( "gridtile" );

			Log.Info( $"Duplicated:{obj.Name}" );
		}
		
	}

	public void HandleRemove( Ray cursorRay )
	{		
		if ( CursorRay(cursorRay).Hit )
		{
			Log.Info( $"Remove {CursorRay( cursorRay ).GameObject.Name}" );
			CursorRay( cursorRay ).GameObject.Destroy();
		}
	}
	public void HandleGetMove( Ray cursorRay )
	{
		if ( CursorRay( cursorRay ).Hit )
		{
			Log.Info( $"Start Moving {CursorRay( cursorRay ).GameObject.Name}" );
			SelectedObject = CursorRay( cursorRay ).GameObject;
			lastRot = SelectedObject.Transform.Rotation;
			beenRotated = false;
		}
	}
	Rotation lastRot;
	bool beenRotated;
	public void HandleMove( Ray cursorRay )
	{
		projectedPoint = ProjectRayOntoGroundPlane( cursorRay.Position, cursorRay.Forward, floors );

		if ( projectedPoint.Hit )
		{
			// Snap the projected point to the grid and adjust for floor height
			var snappedPosition = projectedPoint.EndPosition;

			SelectedObject.Transform.Position = snappedPosition;

			// Only update rotation if 'shouldRotate' is true
			if ( beenRotated )
			{
				SelectedObject.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;
			}
			else
			{
				// Keep the last rotation
				SelectedObject.Transform.Rotation = lastRot;
			}
		}
	}

	public void HandleCopyPlace( SceneTraceResult trace, Ray cursorRay )
	{
		if ( CursorRay( cursorRay ).Hit )
		{
			using var scope = SceneEditorSession.Scope();

			var options = new GameObject.SerializeOptions();
			var selection = CopyObject;
			var json = selection.Serialize( options );
			
			SceneUtility.MakeGameObjectsUnique( json );
			var go = SceneEditorSession.Active.Scene.CreateObject();

			go.Deserialize( json );
			go.MakeNameUnique();
			go.Parent = CurrentGameObjectCollection;
			go.Transform.Position = GetGizmoPosition( trace, cursorRay );
			go.Transform.Rotation = GizmoGameObject.Transform.Rotation;

			go.Tags.Add( "gridtile" );
		}
	}

	public void HandleCopy( Ray cursorRay )
	{
		if ( CursorRay( cursorRay ).Hit )
		{
			if( CursorRay( cursorRay ).GameObject.IsPrefabInstance)
			{
				var prefab = CursorRay( cursorRay ).GameObject.Root;
				CopyObject = prefab;
			}
			else
			{
				CopyObject = CursorRay( cursorRay ).GameObject;
			}

			Log.Info( $"Copy {CopyObject}" );
			beenRotated = false;
		}
	}

	bool _decalX = false;
	bool _decalY = false;
	bool _decalZ = false;
	bool _decalMinZ = false;


	public void DecalScale()
	{
		if ( CurrentPaintMode != PaintMode.Decal ) return;

		if (Gizmo.IsShiftPressed && Application.IsKeyDown( KeyCode.Q ) && !_decalX )
		{
			decalX += 2;
			Log.Info( $"Decal X: {decalX}" );
		}
		else if ( Gizmo.IsAltPressed && Application.IsKeyDown( KeyCode.Q ) && !_decalX )
		{
			decalX -= 2;
			Log.Info( $"Decal XWSWWW: {decalX}" );
		}
		else if ( Gizmo.IsShiftPressed && Application.IsKeyDown( KeyCode.W ) && !_decalY )
		{
			decalY += 2;
			Log.Info( $"Decal Y: {decalY}" );
		}
		else if ( Gizmo.IsAltPressed && Application.IsKeyDown( KeyCode.W ) && !_decalY )
		{
			decalY -= 2;
			Log.Info( $"Decal Y: {decalY}" );
		}
		else if ( Gizmo.IsShiftPressed && Application.IsKeyDown( KeyCode.E ) && !_decalZ )
		{
			decalZ += 2;
			Log.Info( $"Decal Z: {decalZ}" );
		}
		else if ( Gizmo.IsAltPressed && Application.IsKeyDown( KeyCode.E ) && !_decalZ )
		{
			decalZ -= 2;
			Log.Info( $"Decal Z: {decalZ}" );
		}

		_decalX = Application.IsKeyDown( KeyCode.Q );
		_decalY = Application.IsKeyDown( KeyCode.W );
		_decalZ = Application.IsKeyDown( KeyCode.E );
	}
	
	bool _prevlessFloor = false;
	bool _prevmoreFloor = false;
	public void FloorHeightShortCut()
	{
		if ( CurrentPaintMode == PaintMode.Decal ) return;

		if ( Application.IsKeyDown( KeyCode.Q ) && !_prevlessFloor )
		{
			DoFloors( -FloorHeight )();
			so.Delete();
			so = null;
			Grid( new Vector2( 16384, 16384 ), gridRotation, Gizmo.Settings.GridSpacing, SceneViewportWidget.LastSelected.State.GridOpacity );

			if ( floorLabel.IsValid() )
			{
				floorLabel.Text = floorCount.ToString();
			}
			floorcontrolLabel.Text = $"Floor Level: {floorCount}";
		}
		else if ( Application.IsKeyDown( KeyCode.E ) && !_prevmoreFloor )
		{
			DoFloors( FloorHeight )();
			so.Delete();
			so = null;
			Grid( new Vector2( 16384, 16384 ), gridRotation, Gizmo.Settings.GridSpacing, SceneViewportWidget.LastSelected.State.GridOpacity );

			if ( floorLabel.IsValid() )
			{
				floorLabel.Text = floorCount.ToString();
			}
			floorcontrolLabel.Text = $"Floor Level: {floorCount}";
		}

		_prevlessFloor = Application.IsKeyDown( KeyCode.Q );
		_prevmoreFloor = Application.IsKeyDown( KeyCode.E );
	}
	
	//Nasty
	bool _prevlessRotationZ = false;
	bool _prevmoreRotationZ = false;
	bool _prevlessRotationX = false;
	bool _prevmoreRotationX = false;
	bool _prevlessRotationY = false;
	bool _prevmoreRotationY = false;

	public void HandleRotation()
	{

		if ( Application.IsKeyDown( KeyCode.Num1 ) && Gizmo.IsShiftPressed && !_prevlessRotationZ )
		{
			DoRotation( true, GroundAxis.Z )();
			SnapToClosest( GroundAxis.Z );
		}
		else if ( Application.IsKeyDown( KeyCode.Num1 ) && Gizmo.IsAltPressed && !_prevmoreRotationZ )
		{
			DoRotation( false, GroundAxis.Z )();
			SnapToClosest( GroundAxis.Z );
		}

		if ( Application.IsKeyDown( KeyCode.Num2 ) && Gizmo.IsShiftPressed && !_prevlessRotationX )
		{
			DoRotation( true, GroundAxis.X )();
			SnapToClosest( GroundAxis.X );
		}
		else if ( Application.IsKeyDown( KeyCode.Num2 ) && Gizmo.IsAltPressed && !_prevmoreRotationX )
		{
			DoRotation( false, GroundAxis.X )();
			SnapToClosest( GroundAxis.X );
		}

		if ( Application.IsKeyDown( KeyCode.Num3 ) && Gizmo.IsShiftPressed && !_prevlessRotationY )
		{
			DoRotation( true, GroundAxis.Y )();
			SnapToClosest( GroundAxis.Y );
		}
		else if ( Application.IsKeyDown( KeyCode.Num3 ) && Gizmo.IsAltPressed && !_prevmoreRotationY )
		{
			DoRotation( false, GroundAxis.Y )();
			SnapToClosest( GroundAxis.Y );
		}

		_prevlessRotationZ = Application.IsKeyDown( KeyCode.Num1 ) && Gizmo.IsShiftPressed;
		_prevmoreRotationZ = Application.IsKeyDown( KeyCode.Num1 ) && Gizmo.IsAltPressed;
		_prevlessRotationX = Application.IsKeyDown( KeyCode.Num2 ) && Gizmo.IsShiftPressed;
		_prevmoreRotationX = Application.IsKeyDown( KeyCode.Num2 ) && Gizmo.IsAltPressed;
		_prevlessRotationY = Application.IsKeyDown( KeyCode.Num3 ) && Gizmo.IsShiftPressed;
		_prevmoreRotationY = Application.IsKeyDown( KeyCode.Num3 ) && Gizmo.IsAltPressed;
	}

	bool _prevlessRotationSnap = false;
	bool _prevmoreRotationSnap = false;

	public void UpdateRotationSnapWithKeybind()
	{
		if ( Gizmo.IsShiftPressed && Application.IsKeyDown( KeyCode.Num4 ) && !_prevlessRotationSnap )
		{
			if( rotationSnapBox.CurrentIndex != 0)
			rotationSnapBox.CurrentIndex = rotationSnapBox.CurrentIndex - 1;
			rotationLabel.Text = $"Rotation Snap: {rotationSnap}";
		}
		else if ( Gizmo.IsShiftPressed && Application.IsKeyDown( KeyCode.Num5 ) && !_prevmoreRotationSnap )
		{
			if ( rotationSnapBox.CurrentIndex != rotationSnapBox.Count -1 )
				rotationSnapBox.CurrentIndex = rotationSnapBox.CurrentIndex + 1;
			rotationLabel.Text = $"Rotation Snap: {rotationSnap}";
		}

		_prevlessRotationSnap = Gizmo.IsShiftPressed && Application.IsKeyDown( KeyCode.Num4 );
		_prevmoreRotationSnap = Gizmo.IsShiftPressed && Application.IsKeyDown( KeyCode.Num5 );
	}
}
