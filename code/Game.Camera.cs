using Sandbox;
using System.Linq;
using Facepunch.Minigolf.Entities;
using Facepunch.Minigolf.UI;

namespace Facepunch.Minigolf;

public partial class MinigolfGame
{
	Entity StartCameraEntity { get; set; }
	IEnumerable<Ball> ActiveBalls => All.OfType<Ball>().Where( ball => !ball.Cupped );

	private BaseCamera FindActiveCamera()
	{
		if ( Game.LocalClient.Components.Get<DevCamera>() is not null )
			return null;

		// If the game hasn't started yet show our "cinematic" starting camera
		if ( State == GameState.WaitingForPlayers )
		{
			if ( !StartCameraEntity.IsValid() ) StartCameraEntity = All.OfType<StartCamera>().First();
			return StartCameraEntity.Components.Get<BaseCamera>();
		}

		// Pawn will only be set whilst we're in play ( e.g not spectating, or in between holes )
		if ( Game.LocalPawn is Ball ball )
		{
			if ( FreeCamera is not null && ball.CanUseFreeCamera() )
				return FreeCamera;

			FreeCamera = null;

			if ( !ball.Cupped )
				return ball.Camera;

			// Their ball is cupped, lets do the hole end camera cinematic
			HoleEndCamera ??= new HoleEndCamera();
			HoleEndCamera.HolePosition = Course.CurrentHole.GoalPosition;
			return HoleEndCamera;
		}

		// Must be a spectator ( no ball pawn )
		FollowBallCamera ??= new FollowBallCamera();
		FollowBallCamera.Target = ActiveBalls.FirstOrDefault();
		return FollowBallCamera;
	}

	// HoleEndCamera is displayed:
	// 1. After user cups ball
	// 2. After all users have cupped ball
	// 3. On return to lobby
	HoleEndCamera HoleEndCamera;

	public FollowBallCamera FollowBallCamera { get; set; }
	public FreeCamera FreeCamera { get; set; }

	[ClientRpc]
	public void ResetFreeCamera()
	{
		var freeCam = Components.GetOrCreate<FreeCamera>();
		freeCam.Stale = true;
	}
}
