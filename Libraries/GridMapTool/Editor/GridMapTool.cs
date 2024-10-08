using System;
using System.Text.Json.Nodes;
using static Editor.GridMapTool;

namespace Editor;

/// <summary>
/// An editor tool for placing models on a grid.
/// </summary>
[EditorTool]
[Title( "Grid Map Tool" )]
[Icon( "apps", "#F9B003", "#00" )]
[Group( "Grid Map Editor" )]
[Order( 5 )]
public partial class GridMapTool : EditorTool
{
	bool ShouldSnapToGrid = true;
	public PrefabFile PrefabResourse { get; set; }
	public PrefabFile oldresource { get; set; }
	public string SearchString { get; set; } = "";
	public float floors = 0.0f;
	public int floorCount = 0;
	public Rotation rotation = Rotation.From( 90, 0, 0 );
	float rotationSnap = 90.0f;
	public enum PaintMode
	{
		Place = 0,
		Remove = 1,
		Move = 2,
		Copy = 3,
		Duplicate = 4,
		Decal = 5,
	}
	public PaintMode CurrentPaintMode { get; set; } = PaintMode.Place;
	public PaintMode LastPaintMode { get; set; } = PaintMode.Place;

	public GameObject CurrentGameObjectCollection;
	public List<GameObject> GameObjectCollection { get; set; } = new();
	public List<DuplicatedItems> DuplicateObjectCollection { get; set; } = new();

	public struct DuplicatedItems
	{
		public GameObject gameObject;
		public Vector3 position;
		public Rotation rotation;
	}

	TimeSince timeSinceChangedCollection = 0;

	bool gridActiveState = true;

	SerializedObject serializedObject;

	//Get and store the gameobjects from the prefab file.
	GameObject SelectedObject { get; set; }
	GameObject CopyObject { get; set; }

	[Property]
	GameObject GizmoGameObject { get; set; }
	JsonObject SelectedJsonObject { get; set; }
	List<JsonObject> SelectedRandomJsonObject { get; set; } = new();

	//Widget
	public ListView tilelistView { get; set; } = new();

	public List<string> tileGroups { get; set; } = new();
	//

	public List<TileList> tileList { get; set; } = new();
	public struct TileList
	{
		public string name;
		public string group;
		public JsonObject jsonObject;
		public Pixmap icon;
		public List<JsonObject> ranomObjectList;
		public bool isPrefab;
		public bool isRandom;
		public bool isDecal;
	}

	float decalX = 32.0f;
	float decalY = 32.0f;
	float decalZ = 32.0f;

	bool DecalTriPlanar = false;

	public override void OnEnabled()
	{
		gridActiveState = SceneViewportWidget.LastSelected.State.ShowGrid;

		var so = EditorTypeLibrary.GetSerializedObject( this );

		serializedObject = so;

		AllowGameObjectSelection = false;

		foreach ( var objectinscene in Scene.GetAllObjects( true ) )
		{
			if ( objectinscene.Tags.Has( "Collection" ) && (objectinscene.Parent is Scene) )
			{
				collectionDropDown.AddItem( objectinscene.Name );
				GameObjectCollection.Add( objectinscene );
			}
		}

		FloorHeight = (int)ProjectCookie.Get<float>( "GridHeight", 128 );

		MainWindow( so );
		ThreeDGizmo( so );

		UpdateListViewItems();

		ToolWindow( so );
		SceneViewportWidget.LastSelected.State.ShowGrid = false;
		Grid( new Vector2( 16384, 16384 ), gridRotation, Gizmo.Settings.GridSpacing, SceneViewportWidget.LastSelected.State.GridOpacity );
	}

	private void OnSearchTextChanged( string searchText )
	{
		SearchString = searchText.ToLower();
		UpdateListViewItems();
	}

	private void OnGroupChanged()
	{
		UpdateListViewItems();
	}

	private void GroupList()
	{

		tileGroups.Add( "" );
		groupDropDown.AddItem( "" );
	
		foreach ( var item in tileList )
		{
			//Add the groups to the dropdown
			if ( !tileGroups.Contains( item.group ) && item.group != null)
			{
				tileGroups.Add( item.group );
				groupDropDown.AddItem( item.group );
			}
		}
	}

	private void UpdateListViewItems()
	{
		// Get the currently selected group from the dropdown
		var selectedGroup = groupDropDown.CurrentText;

		// Define a lambda function to determine if an item should be included based on the current PaintMode
		Func<TileList, bool> shouldInclude = ( model ) =>
		{
			if ( CurrentPaintMode == PaintMode.Decal )
				return model.isDecal;
			else
				return !model.isDecal;
		};
		Log.Info( CurrentPaintMode );
		// Filter the tile list based on the search string, the selected group, and the PaintMode
		var filteredTileList = tileList
			.Where( model =>
				model.name.ToLower().Contains( SearchString.ToLower() ) &&
				(string.IsNullOrEmpty( selectedGroup ) || model.group == selectedGroup) &&
				shouldInclude( model ) )
			.ToList();

		tilelistView.FocusMode = FocusMode.None;
		tilelistView.SetItems( filteredTileList.Cast<object>() );
		tilelistView.Update(); // Refresh ListView

		oldresource = PrefabResourse;
	}

	public override void OnDisabled()
	{
		base.OnDisabled();

		so.Delete();

		SceneViewportWidget.LastSelected.State.ShowGrid = gridActiveState;

		EndGameObjectGizmo();
	}

	private Vector3 startSelectionPoint;
	private Vector3 endSelectionPoint;
	private bool isSelecting = false;

	bool _prevFilled = false;
	bool _prevGridded = false;
	private void FillSelectionWithTiles( Vector3 start, Vector3 end )
	{
		float gridSpacing = Gizmo.Settings.GridSpacing;
		Vector3 lowerCorner = new Vector3( Math.Min( start.x, end.x ), Math.Min( start.y, end.y ), Math.Min( start.z, end.z ) );
		Vector3 upperCorner = new Vector3( Math.Max( start.x, end.x ), Math.Max( start.y, end.y ), Math.Max( start.z, end.z ) );

		// Calculate the number of tiles in each dimension
		int tilesX = (int)Math.Floor( (upperCorner.x - lowerCorner.x) / gridSpacing ) + 1;
		int tilesY = (int)Math.Floor( (upperCorner.y - lowerCorner.y) / gridSpacing ) + 1;
		int tilesZ = (int)Math.Floor( (upperCorner.z - lowerCorner.z) / gridSpacing ) + 1;

		// Iterate based on the selected axis
		switch ( Axis )
		{
			case GroundAxis.X:
				for ( int i = 0; i < tilesY; i++ )
				{
					for ( int j = 0; j < tilesZ; j++ )
					{
						PlaceTileAtPosition( new Vector3( lowerCorner.x, lowerCorner.y + i * gridSpacing, lowerCorner.z + j * gridSpacing ) );
					}
				}
				break;
			case GroundAxis.Y:
				for ( int i = 0; i < tilesX; i++ )
				{
					for ( int j = 0; j < tilesZ; j++ )
					{
						PlaceTileAtPosition( new Vector3( lowerCorner.x + i * gridSpacing, lowerCorner.y, lowerCorner.z + j * gridSpacing ) );
					}
				}
				break;
			case GroundAxis.Z:
				for ( int i = 0; i < tilesX; i++ )
				{
					for ( int j = 0; j < tilesY; j++ )
					{
						PlaceTileAtPosition( new Vector3( lowerCorner.x + i * gridSpacing, lowerCorner.y + j * gridSpacing, lowerCorner.z ) );
					}
				}
				break;
		}
	}

	private void PlaceTileAtPosition( Vector3 position )
	{

		if ( SelectedRandomJsonObject is not null && SelectedRandomJsonObject.Count != 0 )
		{
			// Create an instance of the Random class
			Random random = new Random();
			Log.Info( "Random object count: " + SelectedRandomJsonObject.Count );
			// Get a random index
			int randomIndex = random.Next( SelectedRandomJsonObject.Count );

			// Select a random tile from the list
			var randomTileJson = SelectedRandomJsonObject[randomIndex];

			// Instantiate the game object
			var go = new GameObject( true, "GridTile" );
			PrefabUtility.MakeGameObjectsUnique( randomTileJson );
			go.Deserialize( randomTileJson );
			go.Parent = CurrentGameObjectCollection;
			go.Transform.Position = position;
			go.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;
			go.Tags.Add( "gridtile" );

			_prevFilled = false;
		}
		if ( SelectedJsonObject != null )
		{
			var go = new GameObject( true, "GridTile" );
			PrefabUtility.MakeGameObjectsUnique( SelectedJsonObject );
			go.Deserialize( SelectedJsonObject );
			go.MakeNameUnique();
			go.Parent = CurrentGameObjectCollection;
			go.Transform.Position = position;
			go.Transform.Rotation = Rotation.FromPitch( -90 ) * rotation;
			go.Tags.Add( "gridtile" );

			_prevFilled = false;
		}
	}

	//GPT Moment
	private SceneTraceResult ProjectRayOntoGroundPlane( Vector3 rayOrigin, Vector3 rayDirection, float groundHeight )
	{
		Vector3 planeNormal;
		Vector3 planePoint;
		GroundAxis axis = Axis;

		switch ( axis )
		{
			case GroundAxis.X:
				planeNormal = Vector3.Forward; // Normal perpendicular to X-axis
				planePoint = new Vector3( SnapToNearestFloor( groundHeight), 0, 0 ); // Point on the X-axis plane
				break;
			case GroundAxis.Y:
				planeNormal = Vector3.Left; // Normal perpendicular to Y-axis
				planePoint = new Vector3( 0, SnapToNearestFloor( groundHeight), 0 ); // Point on the Y-axis plane
				break;
			default: // Z-axis
				planeNormal = Vector3.Up; // Normal perpendicular to Z-axis
				planePoint = new Vector3( 0, 0, SnapToNearestFloor( groundHeight) ); // Point on the Z-axis plane
				break;
		}

		float denom = Vector3.Dot( planeNormal, rayDirection );
		if ( Math.Abs( denom ) > 0.0001f ) // Ensure not parallel
		{
			float num = Vector3.Dot( planeNormal, planePoint - rayOrigin );
			float distance = num / denom;

			if ( distance >= 0 )
			{
				Vector3 hitPoint = rayOrigin + rayDirection * distance;

				// Snap to grid, but do not snap the axis that is currently being used
				switch ( axis )
				{
					case GroundAxis.X:
						hitPoint = new Vector3( hitPoint.x, SnapToGrid( hitPoint.y ), SnapToGrid( hitPoint.z ) );
						break;
					case GroundAxis.Y:
						hitPoint = new Vector3( SnapToGrid( hitPoint.x ), hitPoint.y, SnapToGrid( hitPoint.z ) );
						break;
					default: // Z-axis
						hitPoint = new Vector3( SnapToGrid( hitPoint.x ), SnapToGrid( hitPoint.y ), hitPoint.z );
						break;
				}

				return new SceneTraceResult { Hit = true, EndPosition = hitPoint };
			}
		}

		return new SceneTraceResult { Hit = false };
	}

	private float SnapToNearestFloor( float value)
	{
		// Snap to the nearest height value
		return MathF.Floor( value / FloorHeight ) * FloorHeight;
	}


	private float SnapToGrid( float value )
	{
		if( !ShouldSnapToGrid )
		{
			return value;
		}
		// Assuming you have a grid spacing value
		return MathF.Round( value / Gizmo.Settings.GridSpacing ) * Gizmo.Settings.GridSpacing;
	}

	List<GameObject> SelectedGroupObjects { get; set; } = new List<GameObject>();

	private void SelectObjectsInBox( BBox selectionBox )
	{
		foreach ( var obj in Scene.GetAllObjects( false ) )
		{
			if ( selectionBox.Contains( obj.Transform.Position ) && obj.Tags.Has( "gridtile" ) && !SelectedGroupObjects.Contains(obj) )
			{
				SelectedGroupObjects.Add( obj );
			}
		}

		selectedamount.Text = $"Selected: {SelectedGroupObjects.Count}";
	}

	bool finishedLoadedFromScene = false;

	async Task LoadFromScene()
	{
		if ( !finishedLoadedFromScene && !loadscene )
		{
			var gameObject = SceneUtility.GetPrefabScene( PrefabResourse ).Clone( new Transform( Vector3.Up * 100000 ) );
			gameObject.BreakFromPrefab();
			gameObject.Flags = GameObjectFlags.NotSaved | GameObjectFlags.Hidden | GameObjectFlags.Loading;
			
			var allObjects = gameObject.GetAllObjects( true );

			bool isFirst = true;
			GameObject lastFoundObject = null;
			foreach ( var obj in allObjects )
			{
				// Skip the first object
				if ( isFirst )
				{
					isFirst = false;
					continue;
				}
					if ( !tileList.Any( x => x.name == obj.Name ) && !obj.Tags.Has( "ignore" ) && !obj.IsAncestor( lastFoundObject) )
					{
						if ( !obj.Tags.Has( "random" ) && obj.Tags.Has( "group" ) && obj.Components.Get<ModelRenderer>( FindMode.EverythingInSelf ) != null )
						{
							lastFoundObject = obj;
							tileList.Add( new TileList()
							{
								name = obj.Name,
								group = obj.Parent.Name,
								jsonObject = obj.Serialize(),
								isPrefab = obj.IsPrefabInstance,
								icon = AssetSystem.FindByPath( obj.Components.Get<ModelRenderer>( FindMode.EnabledInSelfAndChildren ).Model.ResourcePath ).GetAssetThumb()
							} ); 
							//Log.Info( obj.Parent.Name );
						}
						else if ( !obj.Tags.Has( "random" ) && obj.Tags.Has( "decal" ) && obj.Components.Get<DecalRenderer>( FindMode.EnabledInSelf ) != null )
						{
							lastFoundObject = obj;
							tileList.Add( new TileList()
							{
								name = obj.Name,
								jsonObject = obj.Serialize(),
								isPrefab = obj.IsPrefabInstance,
								icon = AssetSystem.FindByPath( obj.Components.Get<DecalRenderer>( FindMode.EnabledInSelfAndChildren ).Material.ResourcePath ).GetAssetThumb(),
								isDecal = true
							} );
							//Log.Info( obj.Components.Get<ModelRenderer>( FindMode.EnabledInSelfAndChildren ).Model );
						}
						else if ( !obj.Tags.Has( "random" ) && !obj.Tags.Has( "group" ) && obj.Components.Get<ModelRenderer>( FindMode.EnabledInSelf ) != null )
						{
							lastFoundObject = obj;
							tileList.Add( new TileList()
							{
								name = obj.Name,
								jsonObject = obj.Serialize(),
								isPrefab = obj.IsPrefabInstance,
								icon = AssetSystem.FindByPath( obj.Components.Get<ModelRenderer>( FindMode.EnabledInSelfAndChildren ).Model.ResourcePath ).GetAssetThumb()
							} );

							//Log.Info( obj.Components.Get<ModelRenderer>( FindMode.EnabledInSelfAndChildren ).Model );
						}
						else if ( obj.Tags.Has( "random" ) && !obj.Tags.Has( "group" ) )
						{
							lastFoundObject = obj;

							var randList = new List<JsonObject>();

							foreach ( var randobj in obj.Children )
							{
								randList.Add( randobj.Serialize() );
								//Log.Info( randobj.Name );
							}

							tileList.Add (new TileList()
							{
								name = obj.Name,
								//group = obj.Parent.Name,
								//jsonObject = obj.Serialize(),
								icon = AssetSystem.FindByPath( obj.Components.Get<ModelRenderer>( FindMode.EnabledInSelfAndChildren ).Model.ResourcePath ).GetAssetThumb(),
								isRandom = true,
								isPrefab = obj.IsPrefabInstance,
								ranomObjectList = new (randList)
							});

							//Log.Info( randList.FirstOrDefault().ToString() );
						}
						//await Task.Delay( 10 );

				}
			}
			GroupList();
			UpdateListViewItems();
			finishedLoadedFromScene = true;
			gameObject.Destroy();
		}
	}

	bool loadscene;

	Vector3 gridRotation = Vector3.Up;
	public override void OnUpdate()
	{
		Grid( new Vector2( 16384, 16384 ), gridRotation, Gizmo.Settings.GridSpacing, SceneViewportWidget.LastSelected.State.GridOpacity );

		UpdateWidgetValues();

		gizmowidg.CameraRotation = Gizmo.Camera.Rotation.RotateAroundAxis( Vector3.Backward, 90 ).Inverse;

		gizmowidg.Update();

		if ( PrefabResourse is not null && tileList is not null)
		{
			if ( PrefabResourse.ResourceName.Contains( "_tileset" ) )
			{
				_ = LoadFromScene();
				loadscene = true;
			}
		}
				
		if ( GameObjectCollection is not null && PrefabResourse is not null )
		{
			CurrentGameObjectCollection = GameObjectCollection.FirstOrDefault( x => x.Name == collectionDropDown.CurrentText );

			collectionDropDown.ItemChanged += () =>
			{
				timeSinceChangedCollection = 0;
			};
		}
		
		if ( PrefabResourse != oldresource )
		{
			Log.Info( "Resource Changed" );
			tileList.Clear();
			UpdateListViewItems();
		}

		//tilelistView.ItemsSelected = SetSelection;

		// Do gizmos and stuff
		var cursorRay = Gizmo.CurrentRay;

		var tr = SceneEditorSession.Active.Scene.Trace
					.Ray( cursorRay, 5000 )
						.UseRenderMeshes( true )
						.UsePhysicsWorld( false )
						.WithoutTags( "gridtile" )
						.Run();

		var boxtr = SceneEditorSession.Active.Scene.Trace
			.Ray( cursorRay, 5000 )
				.UsePhysicsWorld( true )
				.WithoutTags( "gridtile" )
				.Run();

		if ( !boxtr.Hit )
		{
			Vector3 rayOrigin = cursorRay.Position;
			Vector3 rayDirection = cursorRay.Forward;

			boxtr = ProjectRayOntoGroundPlane( rayOrigin, rayDirection, 0 );
		}
		
		GroundGizmo( cursorRay );

		HandleRotation();

		PaintGizmos( tr );

		DecalScale();

		if ( Gizmo.IsShiftPressed && SelectedObject is null )
		{
			projectedPoint = ProjectRayOntoGroundPlane( cursorRay.Position, cursorRay.Forward, floors );
			if ( Gizmo.WasLeftMousePressed && CurrentPaintMode != PaintMode.Duplicate )
			{
				var snappedPosition = projectedPoint.EndPosition;

				startSelectionPoint = snappedPosition;

				isSelecting = true;
			}
			else if ( isSelecting )
			{
				var snappedPosition = projectedPoint.EndPosition;

				endSelectionPoint = snappedPosition;

				switch ( Axis )
				{
					case GroundAxis.X:
						endSelectionPoint.x += FloorHeight;
						break;
					case GroundAxis.Y:
						endSelectionPoint.y += FloorHeight;
						break;
					default:
						endSelectionPoint.z += FloorHeight;
						break;
				}

				if ( Gizmo.WasLeftMouseReleased )
				{
					var bbox = new BBox( startSelectionPoint, endSelectionPoint );
					SelectObjectsInBox( bbox );
					isSelecting = false;
				}
			}

			if ( Application.IsKeyDown( KeyCode.F ) && !_prevFilled )
			{
				FillSelectionWithTiles( startSelectionPoint, endSelectionPoint );
				isSelecting = false;
			}

			_prevFilled = Application.IsKeyDown( KeyCode.F );

			if ( Application.IsKeyDown( KeyCode.G ) && !_prevGridded )
			{
				ShouldSnapToGrid = !ShouldSnapToGrid;
			}

			_prevGridded = Application.IsKeyDown( KeyCode.G );

			if ( Application.IsKeyDown( KeyCode.R ) )
			{
				foreach ( var obj in SelectedGroupObjects )
				{
					obj.Destroy();
				}
				SelectedGroupObjects.Clear();
				selectedamount.Text = $"Selected: {SelectedGroupObjects.Count}";
			}

			if ( Application.IsKeyDown( KeyCode.T ) )
			{
				if ( CurrentPaintMode != PaintMode.Duplicate )
				{
					LastPaintMode = CurrentPaintMode;
				}

				foreach ( var obj in SelectedGroupObjects )
				{
					DuplicateObjectCollection.Add( new DuplicatedItems()
					{
						gameObject = obj,
						position = obj.Transform.Position,
						rotation = obj.Transform.Rotation
					} );
				}
				
				EndGameObjectGizmo();

				CurrentPaintMode = PaintMode.Duplicate;
			}
			if ( CurrentPaintMode == PaintMode.Duplicate && DuplicateObjectCollection != null )
			{
				HandleDuplicate(tr ,cursorRay );
			}
			
			if( Gizmo.WasLeftMousePressed && CurrentPaintMode == PaintMode.Duplicate )
			{
				HandlePlaceDuplicatedGroup(  tr, cursorRay );
			}

			if ( Application.IsKeyDown( KeyCode.Z ) )
			{
				Axis = GroundAxis.Z;
				gridRotation = Vector3.Up;
				so.Delete();
				so = null;
				Grid( new Vector2( 16384, 16384 ), gridRotation, Gizmo.Settings.GridSpacing, SceneViewportWidget.LastSelected.State.GridOpacity );
			}
			else if ( Application.IsKeyDown( KeyCode.C ) )
			{
				Axis = GroundAxis.X;
				gridRotation = Vector3.Left;
				so.Delete();
				so = null;
				Grid( new Vector2( 16384, 16384 ), gridRotation, Gizmo.Settings.GridSpacing, SceneViewportWidget.LastSelected.State.GridOpacity );
			}
			else if ( Application.IsKeyDown( KeyCode.X ) )
			{
				Axis = GroundAxis.Y;
				gridRotation = Vector3.Forward;
				so.Delete();
				so = null;
				Grid( new Vector2( 16384, 16384 ), gridRotation, Gizmo.Settings.GridSpacing, SceneViewportWidget.LastSelected.State.GridOpacity );
			}

			FloorHeightShortCut();

			if ( SelectedGroupObjects is not null )
			{
				using ( Gizmo.Scope( "Selection" ) )
				{
					foreach ( var obj in SelectedGroupObjects )
					{
						Gizmo.Draw.Color = Color.Red;
						Gizmo.Draw.LineBBox( obj.GetBounds() );
					}
				}
			}
			using ( Gizmo.Scope( "selection_box" ) )
			{
				var rect = new BBox( startSelectionPoint, endSelectionPoint );
				Gizmo.Draw.Color = Color.Blue.WithAlpha( 0.25f );
				Gizmo.Draw.SolidBox( rect );
				Gizmo.Draw.Color = Color.Blue;
				Gizmo.Draw.LineBBox( rect );
			}
			UpdateRotationSnapWithKeybind();
			return;
		}
		else if ( isSelecting )
		{
			isSelecting = false;
		}
		else
		{
			if ( SelectedGroupObjects != null )
			{
				SelectedGroupObjects.Clear();
			}
			startSelectionPoint = Vector3.Zero;
			endSelectionPoint = Vector3.Zero;

			selectedamount.Text = $"Selected: {SelectedGroupObjects.Count}";
			
			EndDuplicateGizmo();
		}

		if ( !Gizmo.IsShiftPressed && CurrentPaintMode == PaintMode.Duplicate )
		{
			//Log.Info( LastPaintMode );

			CurrentPaintMode = LastPaintMode;
		}

		if ( !Gizmo.IsCtrlPressed )
		{
			if ( Gizmo.WasLeftMousePressed && CurrentPaintMode == PaintMode.Place )
			{
				HandlePlacement( tr, cursorRay );
			}
			else if ( Gizmo.WasLeftMousePressed && CurrentPaintMode == PaintMode.Decal )
			{
				HandleDecalPlace();
			}
			else if ( Gizmo.WasLeftMousePressed && CurrentPaintMode == PaintMode.Remove )
			{
				HandleRemove( cursorRay );
			}
			if ( Gizmo.WasLeftMousePressed && CurrentPaintMode == PaintMode.Move )
			{
				HandleGetMove( cursorRay );
			}
			else if ( Gizmo.WasLeftMouseReleased && CurrentPaintMode == PaintMode.Move && SelectedObject is not null )
			{
				SelectedObject = null;
			}

			if ( Gizmo.IsLeftMouseDown && CurrentPaintMode == PaintMode.Move && SelectedObject is not null )
			{
				HandleMove( cursorRay );
			}

			if ( Gizmo.WasLeftMouseReleased && CurrentPaintMode == PaintMode.Copy && CopyObject == null )
			{
				HandleCopy( cursorRay );
			}

			if ( Gizmo.WasLeftMousePressed && CurrentPaintMode == PaintMode.Copy && CopyObject != null )
			{
				HandleCopyPlace( tr, cursorRay );
			}

			if ( Application.IsKeyDown( KeyCode.Escape ) )
			{
				CopyObject = null;
				SelectedJsonObject = null;
				//SelectedRandomJsonObject.Clear();
				SelectedRandomJsonObject = null;
				EndGameObjectGizmo();
			}
		}

		if ( CurrentGameObjectCollection is not null )
		{
			CollectionGroupHighLight();
		}

		if ( !Gizmo.IsLeftMouseDown )
			return;
	}

	void SetSelection( object o )
	{	
		if ( o is JsonObject s )
		{
			SelectedJsonObject = s;

			Log.Info( $"Selected {s}" );
		}
	}
	private Action DoRotation( bool leftright, GroundAxis axis )
	{
		beenRotated = true;	
		
		float rotationIncrement = leftright ? rotationSnap : -rotationSnap;
		return () =>
		{

			switch ( axis )
			{
				case GroundAxis.X:
					rotation *= Rotation.FromAxis( Vector3.Right, rotationIncrement );
					break;
				case GroundAxis.Y:
					rotation *= Rotation.FromAxis( Vector3.Forward, rotationIncrement );
					break;
				case GroundAxis.Z:
					rotation *= Rotation.FromAxis( Vector3.Up, rotationIncrement );
					break;
			}

		};
	}

	void SnapToClosest( GroundAxis axis )
	{
		var a = rotation.Angles();

		switch (axis)
		{
			case GroundAxis.X:
				a.pitch = a.pitch.SnapToGrid( rotationSnap );
				break;
			case GroundAxis.Y:
				a.yaw = a.yaw.SnapToGrid( rotationSnap );
				break;
			case GroundAxis.Z:
				a.roll = a.roll.SnapToGrid( rotationSnap );
				break;
		}
		rotation = Rotation.From( a );
	}


	
private Action DoFloors( int i )
	{
		return () =>
		{
			floors += i;
			if ( i > 0 )
			{
				floorCount++;
			}
			else if ( i < 0 )
			{
				floorCount--;
			}
		};
	}

	public void CreateCollection( string name )
	{
		using var scope = SceneEditorSession.Scope();
		var go = new GameObject( true, name );
		go.Transform.Position = Vector3.Zero;
		go.Tags.Add( "collection" );

		GameObjectCollection.Add( go );

		Log.Info( "Created collection" );
	}

	void ClearAll()
	{
		foreach ( var obj in Scene.GetAllObjects( false ).Where( x => x.Tags.Has( "gridtile" ) ).ToArray() )
		{
			obj.Destroy();
		}
	}
}
