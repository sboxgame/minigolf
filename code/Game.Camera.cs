using Sandbox;
using System.Linq;

using Facepunch.Minigolf.Entities;
using Facepunch.Minigolf.UI;

namespace Facepunch.Minigolf;

public partial class Game
{
	Entity StartCameraEntity { get; set; }
	IEnumerable<Ball> ActiveBalls => All.OfType<Ball>().Where( ball => !ball.Cupped );

	public virtual BaseCamera FindActiveCamera()
	{
		// You can use dev cam
		var devCam = Sandbox.Game.LocalClient.Components.Get<DevCamera>();
		if ( devCam != null ) return null;

		// If the game hasn't started yet show our "cinematic" starting camera
		if ( State == GameState.WaitingForPlayers )
		{
			if ( !StartCameraEntity.IsValid() ) StartCameraEntity = Entity.All.OfType<StartCamera>().First();
			return StartCameraEntity.Components.Get<BaseCamera>();
		}

		// Pawn will only be set whilst we're in play ( e.g not spectating, or in between holes )
		if ( Sandbox.Game.LocalPawn is Ball ball )
        {
			if ( FreeCamera != null )
			{
				if ( !ball.InPlay && !ball.Cupped && FreeCamTimeLeft > 0.0f ) return FreeCamera;

				// Bad conditions, set to null and fall through
				FreeCamera = null;
			}

			if ( !ball.Cupped ) return ball.Camera;

			// Their ball is cupped, lets do the hole end camera cinematic
			if ( HoleEndCamera == null )
			{
				HoleEndCamera = new( Course.CurrentHole.GoalPosition );
			}

			return HoleEndCamera;
		}

		// Must be a spectator ( no ball pawn )

		// TODO: Allow more freedom in spectate, cycle between players, etc
		if ( FollowBallCamera == null )
		{
			FollowBallCamera = new();
		}

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
	public float FreeCamTimeLeft { get; set; } = 30.0f;

	[Event.Client.Frame]
	public void TickFreeCamTimeLeft()
	{
		if ( FreeCamera != null )
			FreeCamTimeLeft -= RealTime.Delta;
	}
}
