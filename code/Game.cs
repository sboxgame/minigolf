using System.Linq;
using Sandbox;

namespace Minigolf
{
	[Library( "Minigolf" ), Hammer.Skip] // TODO: This is a hacky way to set our currently playing properly.
	public partial class Game : GameBase
	{
		public static Game Current { get; protected set; }
		public Hud Hud { get; private set; }

		public Game()
		{
			Current = this;
			Transmit = TransmitType.Always;

			if ( IsServer )
			{
				AddToPrecache();
				Course = new();
			}

			if ( IsClient )
			{
				Hud = new Hud();
				Local.Hud = Hud;
			}
		}

		public override void Shutdown()
		{
			if ( Current == this )
			{
				Current = null;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( Local.Hud == Hud )
			{
				Local.Hud = null;
			}

			Hud?.Delete();
			Hud = null;
		}

		public override void ClientJoined( Client cl )
		{
			Log.Info( $"\"{cl.Name}\" has joined the game" );

			if ( State == GameState.Playing )
			{
				cl.SetValue( "late", true );
				ChatBox.AddInformation( To.Everyone, $"{cl.Name} has joined late, they will not be eligible for scoring.", $"avatar:{cl.SteamId}" );

				// Just give them shitty scores on each hole for now
				for ( int i = 1; i <= Course.CurrentHole.Number; i++ )
					cl.SetInt( $"par_{i}", Course.Holes[i].Par + 1 );
			}
			else
			{
				ChatBox.AddInformation( To.Everyone, $"{cl.Name} has joined", $"avatar:{cl.SteamId}" );
			}
		}
		
		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			Log.Info( $"\"{cl.Name}\" has left the game ({reason})" );
			ChatBox.AddInformation( To.Everyone, $"{cl.Name} has left ({reason})", $"avatar:{cl.SteamId}" );

			if ( cl.Pawn.IsValid() )
			{
				cl.Pawn.Delete();
				cl.Pawn = null;
			}
		}

		public override bool CanHearPlayerVoice( Client source, Client dest )
		{
			return true;
		}

		public override void PostLevelLoaded()
		{
			StartTime = Time.Now + 60.0f;
			// Replaced with [Event.Entity.PostLoaded] since for some reason it runs clientside and this doesn't.
		}

		public ICamera FindActiveCamera()
		{
			// If the game hasn't started yet show our "cinematic" camera
			if ( State == GameState.WaitingForPlayers )
			{
				// todo: cache ref and fuckin ICamera it up
				var cameraEnt = Entity.All.OfType<StartCamera>().First();
				if ( cameraEnt == null )
					return null;

				StaticCamera camera = new( cameraEnt.Position, cameraEnt.Rotation.Angles(), cameraEnt.FOV );
				return camera;
			}

			if ( FreeCamera != null )
			{
				if ( Local.Pawn is Ball balll && !balll.InPlay && !balll.Cupped && FreeCamTimeLeft > 0.0f )
				{
					return FreeCamera;
				}

				FreeCamera = null;
			}

			if ( Local.Pawn is Ball ball )
			{
				BallCamera.Ball = ball;
				return BallCamera;
			}

			// if they have no pawn and the game is active, they must be a spectator

			// matt: Is this used internally? probably not needed
			// if ( Local.Client.Camera != null ) return Local.Client.Camera;
			// if ( Local.Pawn != null ) return Local.Pawn.Camera;

			return null;
		}

		public FollowBallCamera BallCamera = new();
		public FreeCamera FreeCamera { get; set; }
		public float FreeCamTimeLeft { get; set; } = 30.0f;

		[Event.Frame]
		public void TickFreeCamTimeLeft()
		{
			if ( FreeCamera != null )
				FreeCamTimeLeft -= RealTime.Delta;
		}

		public override void BuildInput( InputBuilder input )
		{
			Host.AssertClient();

			Event.Run( "buildinput", input );

			// todo: pass to spectate

			if ( input.Pressed( InputButton.View ) && Local.Pawn.IsValid() && !(Local.Pawn as Ball).InPlay && !(Local.Pawn as Ball).Cupped && FreeCamTimeLeft > 0.0f )
			{
				if ( FreeCamera == null )
					FreeCamera = new FreeCamera();
				else
					FreeCamera = null;
			}

			// the camera is the primary method here
			var camera = FindActiveCamera();
			camera?.BuildInput( input );

			Local.Pawn?.BuildInput( input );
		}

		public override CameraSetup BuildCamera( CameraSetup camSetup )
		{
			var cam = FindActiveCamera();
			cam?.Build( ref camSetup );

			PostCameraSetup( ref camSetup );

			return camSetup;
		}

		public override void OnVoicePlayed( ulong steamId, float level )
		{
			VoiceList.Current?.OnVoicePlayed( steamId, level );
		}

		/// <summary>
		/// Called each tick.
		/// Serverside: Called for each client every tick
		/// Clientside: Called for each tick for local client. Can be called multiple times per tick.
		/// </summary>
		public override void Simulate( Client cl )
		{
			if ( !cl.Pawn.IsValid() ) return;

			// Block Simulate from running clientside
			// if we're not predictable.
			if ( !cl.Pawn.IsAuthority ) return;

			cl.Pawn.Simulate( cl );

			if ( cl.Pawn is Ball ball && !ball.Cupped )
			{
				if ( Input.Pressed( InputButton.Reload ) )
					ResetBall( cl );
			}
		}

		/// <summary>
		/// Called each frame on the client only to simulate things that need to be updated every frame. An example
		/// of this would be updating their local pawn's look rotation so it updates smoothly instead of at tick rate.
		/// </summary>
		public override void FrameSimulate( Client cl )
		{
			Host.AssertClient();

			if ( !cl.Pawn.IsValid() ) return;

			// Block Simulate from running clientside
			// if we're not predictable.
			// matt: do we use FrameSimulateclientside
			if ( !cl.Pawn.IsAuthority ) return;

			cl.Pawn?.FrameSimulate( cl );
		}
	}
}
