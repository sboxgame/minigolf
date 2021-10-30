using Sandbox;
using System;
using System.Collections.Generic;

namespace Minigolf
{
	[Library( "minigolf_water" )]
	[Hammer.Solid]
	public partial class Water : AnimEntity
	{
		public WaterController WaterController = new WaterController();

		public Water()
		{
			WaterController.WaterEntity = this;

			Transmit = TransmitType.Always;

			EnableTouch = true;
			EnableTouchPersists = true;
		}

		public override void Spawn()
		{
			base.Spawn();

			CreatePhysics();
		}

		void CreatePhysics()
		{
			var PhysGroup = SetupPhysicsFromModel( PhysicsMotionType.Static );
			PhysGroup?.SetSurface( "water" );

			ClearCollisionLayers();
			AddCollisionLayer( CollisionLayer.Water );
			AddCollisionLayer( CollisionLayer.Trigger );
			EnableSolidCollisions = false;
			EnableTouch = true;
			EnableTouchPersists = true;
		}

		public override void Touch( Entity other )
		{
			base.Touch( other );

			WaterController.Touch( other );
		}

		public override void EndTouch( Entity other )
		{
			base.EndTouch( other );

			WaterController.EndTouch( other );
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			WaterController.StartTouch( other );

			if ( other is not Ball ball )
				return;

			Sound.FromWorld( "minigolf.ball_in_water", ball.Position );

			Action task = async () =>
			{
				await Task.DelaySeconds( 2 );

				if ( !other.IsValid() )
					return;

				// TODO: check if other is still in water

				using ( Prediction.Off() )
				{
					if ( other is Ball ball )
						Game.Current.BallOutOfBounds( ball, Game.OutOfBoundsType.Water );
				}
			};
			task.Invoke();
		}
	}
}
