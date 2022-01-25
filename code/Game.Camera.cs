using Sandbox;
using System.Linq;

using Facepunch.Minigolf.Entities;
using Facepunch.Minigolf.UI;

namespace Facepunch.Minigolf;

public partial class Game
{
	public override ICamera FindActiveCamera()
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

		if ( Local.Pawn is Ball ball && !ball.Cupped )
		{
			HoleEndCamera = null;
			BallCamera.Ball = ball;
			return BallCamera;
		}

		if ( HoleEndCamera == null )
		{
			HoleEndCamera = new( Course.CurrentHole.GoalPosition );
		}

		return HoleEndCamera;

		// if they have no pawn and the game is active, they must be a spectator

		// HoleEndCamera used if there's no spectating going on I guess

		return null;
	}

	// HoleEndCamera is displayed:
	// 1. After user cups ball
	// 2. After all users have cupped ball
	// 3. On return to lobby
	HoleEndCamera HoleEndCamera;

	public FollowBallCamera BallCamera = new();
	public FreeCamera FreeCamera { get; set; }
	public float FreeCamTimeLeft { get; set; } = 30.0f;

	[Event.Frame]
	public void TickFreeCamTimeLeft()
	{
		if ( FreeCamera != null )
			FreeCamTimeLeft -= RealTime.Delta;
	}

	public override CameraSetup BuildCamera(CameraSetup camSetup)
	{
		var cam = FindActiveCamera();
		cam?.Build(ref camSetup);

		PostCameraSetup(ref camSetup);

		return camSetup;
	}
}