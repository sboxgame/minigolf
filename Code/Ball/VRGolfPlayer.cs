using Sandbox;
using Sandbox.VR;
using System.Diagnostics;

public sealed class VRGolfPlayer : Component
{
	public float ShotPower { get; set; } = 0f;

	[Property] GameObject Head { get; set; }

	[Property] GameObject GolfPutter { get; set; }

	[Property] GameObject PutterHead { get; set; }

	private Vector3 lastFramePosition;
	private List<Vector3> velocityBuffer = new List<Vector3>();
	private const int velocityBufferSize = 5;

	public bool RightDominant { get; set; } = true;

	RealTimeSince timeSinceOutOfPlay;

	SkinnedModelRenderer golfPutterRenderer;

	[Sync] Vector3 HeadPosition { get; set; }
	[Sync] Rotation HeadRotation { get; set; }

	[Sync] Vector3 PutterPosition { get; set; }
	[Sync] Rotation PutterRotation { get; set; }

	VRController PrimaryHand => RightDominant ? Input.VR.RightHand : Input.VR.LeftHand;

	List<GameObject> UIPanels { get; set; } = new List<GameObject>();

	public bool MenuMode { get; set; }

	protected override async void OnStart()
	{
		if ( !Game.IsRunningInVR )
		{
			DestroyGameObject();
			return;
		}

		golfPutterRenderer = GolfPutter.GetComponent<SkinnedModelRenderer>();

		if ( IsProxy )
		{
			Head.GetComponentInChildren<ModelRenderer>().RenderType = ModelRenderer.ShadowRenderType.On;
		}
		else
		{
			foreach ( var item in Scene.GetAll<ScreenPanel>() )
			{
				UIPanels.Add( item.GameObject );
				var worldpanel = item.AddComponent<Sandbox.WorldPanel>();
				worldpanel.PanelSize = new Vector2( 102.4f, 51.2f );
				worldpanel.RenderScale = 0.075f;
				worldpanel.RenderOptions.Overlay = true;
				item.Destroy();
			}

			await Task.DelaySeconds( 1f );

			foreach ( var item in UIPanels )
			{
				item.Enabled = false;
				await Task.Frame();
				item.Enabled = true;
			}
		}
	}

	protected override void OnPreRender()
	{
		if ( IsProxy )
		{
			Head.LocalPosition = Vector3.Lerp( Head.LocalPosition, HeadPosition, 0.9f );
			Head.LocalRotation = Rotation.Lerp( Head.LocalRotation, HeadRotation, 0.9f );

			GolfPutter.LocalPosition = Vector3.Lerp( GolfPutter.LocalPosition, PutterPosition, 0.9f );
			GolfPutter.LocalRotation = Rotation.Lerp( GolfPutter.LocalRotation, PutterRotation, 0.9f );
		}
		else
		{
			HeadPosition = Head.LocalPosition;
			HeadRotation = Head.LocalRotation;

			PutterPosition = GolfPutter.LocalPosition;
			PutterRotation = GolfPutter.LocalRotation;

			if ( !MenuMode )
			{
				foreach ( var item in UIPanels )
				{
					item.WorldPosition = PrimaryHand.Transform.Position;
					item.WorldRotation = PrimaryHand.Transform.Rotation * Rotation.FromYaw( -90f * (RightDominant ? -1f : 1f) ) * Rotation.FromRoll( -90f * (RightDominant ? -1f : 1f) );
				}
			}
			else
			{
				foreach ( var item in UIPanels )
				{
					item.WorldPosition = WorldPosition + WorldRotation.Forward * 39f + Vector3.Up * 39f;
					var worldpanel = item.GetComponent<Sandbox.WorldPanel>();
					worldpanel.PanelSize = new Vector2( 1024f, 512f );
					worldpanel.RenderScale = 0.5f;
					item.WorldRotation = WorldRotation * Rotation.FromPitch( -35f );
				}
			}

			GolfPutter.Transform.World = PrimaryHand.Transform;
			GolfPutter.WorldRotation *= Rotation.FromPitch( 35f );

		}
	}

	Sandbox.UI.WorldInput worldInput;

	public void DoWorldInput()
	{
		if ( worldInput == null )
		{
			worldInput = new Sandbox.UI.WorldInput();
			worldInput.Enabled = true;
		}

		var inputRay = new Ray( PrimaryHand.Transform.Position, PrimaryHand.Transform.Rotation.Forward );

		worldInput.Ray = inputRay;

		if ( worldInput.Hovered.FindRootPanel() is Sandbox.UI.WorldPanel WP && WP.RayToLocalPosition( inputRay, out Vector2 pos, out float paneldistance ) )
		{
			Gizmo.Draw.Line( inputRay.Position, inputRay.Position + inputRay.Forward * paneldistance );
		}
		else
		{
			Gizmo.Draw.Line( inputRay.Position, inputRay.Position + inputRay.Forward * 100f );
		}

		worldInput.MouseLeftPressed = PrimaryHand.Trigger.Value > 0.5f;
	}

	protected override void OnFixedUpdate()
	{
		if ( !Ball.Local.IsValid() )
		{
			MenuMode = true;
		}

		if ( IsProxy )
		{
			return;
		}

		if ( Input.VR.LeftHand.ButtonA.IsPressed )
		{
			RightDominant = false;
		}

		if ( Input.VR.RightHand.ButtonA.IsPressed )
		{
			RightDominant = true;
		}

		GolfPutter.Transform.World = PrimaryHand.Transform;
		GolfPutter.WorldRotation *= Rotation.FromPitch( 35f );

		if ( MenuMode )
		{
			DoWorldInput();
			return;
		}

		var currentFrameVelocity = PutterHead.WorldPosition - lastFramePosition;

		velocityBuffer.Add( currentFrameVelocity );

		if ( velocityBuffer.Count > velocityBufferSize )
		{
			velocityBuffer.RemoveAt( 0 );
		}

		var tr = Scene.Trace.Ray( GolfPutter.WorldPosition - GolfPutter.WorldRotation.Forward * 7.5f, GolfPutter.WorldPosition + GolfPutter.WorldRotation.Forward * 90f ).Run();

		if ( tr.Hit )
		{
			golfPutterRenderer.Set( "putter_length", tr.Distance * 2f );
		}

		var averagedVelocity = Vector3.Zero;
		foreach ( var velocity in velocityBuffer )
		{
			averagedVelocity += velocity;
		}

		averagedVelocity /= velocityBuffer.Count;

		Ball.Local.Controller.Camera.Enabled = false;

		var goToBall = !Ball.Local.Controller.InPlay || Input.VR.LeftHand.Trigger.Value > 0.5f || Input.VR.RightHand.Trigger.Value > 0.5f;

		if ( goToBall )
		{
			WorldPosition = Ball.Local.WorldPosition;
		}

		if ( Ball.Local.Controller.InPlay )
		{
			timeSinceOutOfPlay = 0f;
			ShotPower = 0f;
		}

		if ( timeSinceOutOfPlay < 0.5f )
		{
			golfPutterRenderer.Tint = Color.White.WithAlpha( 0.5f );
		}
		else
		{
			golfPutterRenderer.Tint = Color.White.WithAlpha( 1f );
		}

		var putterHeadForward = Vector3.Dot( averagedVelocity, PutterHead.WorldRotation.Forward ) > 0 ? PutterHead.WorldRotation.Forward.WithZ( 0 ) : PutterHead.WorldRotation.Backward.WithZ( 0 );

		var direction = (averagedVelocity.WithZ( 0 ) + putterHeadForward).Normal;
		var ballRadius = Ball.Local.Rigidbody.PhysicsBody.GetBounds().Size.z / 2;

		var steps = 10;
		var stepSize = 1f / steps;

		bool hitBallInbetween = false;

		for ( int i = 0; i < steps; i++ )
		{
			var interpolatedPosition = Vector3.Lerp( lastFramePosition, PutterHead.WorldPosition, i * stepSize );

			if ( Vector3.DistanceBetween( interpolatedPosition, Ball.Local.WorldPosition ) < ballRadius * 2f )
			{
				hitBallInbetween = true;
				break;
			}
		}

		if ( timeSinceOutOfPlay > 0.5f && (Vector3.DistanceBetween( PutterHead.WorldPosition, Ball.Local.WorldPosition ) < ballRadius * 2f || hitBallInbetween) )
		{
			ShotPower = Math.Clamp( averagedVelocity.Length / 7.5f, 0f, 1f );
			PrimaryHand.TriggerHaptics( HapticEffect.SoftImpact );
		}

		if ( ShotPower > 0.0f && !Ball.Local.Controller.InPlay )
		{
			Ball.Local.Stroke( Rotation.LookAt( direction ).Yaw(), ShotPower );

			Ball.Local.Controller.InPlay = true;
			Ball.Local.Controller.LastShotPower = ShotPower;
			ShotPower = 0f;

			IGameEvent.Post( x => x.BallStroke( Ball.Local ) );
		}

		lastFramePosition = PutterHead.WorldPosition;
	}
}
