using Sandbox;
using System;
using System.Collections.Generic;

namespace Minigolf
{
	[Library( "minigolf_water" )]
	public partial class Water : AnimEntity
	{
		public WaterController WaterController = new WaterController();

		public Water()
		{
			WaterController.WaterEntity = this;

			Transmit = TransmitType.Always;

			if ( IsClient )
			{
				CreatePhysics();
				SceneLayer = "water";
			}

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

			// WaterController.Touch( other );
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
