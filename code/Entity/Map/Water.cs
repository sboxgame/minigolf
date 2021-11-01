using Sandbox;
using System;
using System.Collections.Generic;

namespace Minigolf
{
	[Library( "minigolf_water" )]
	[Hammer.Solid]
	[Hammer.AutoApplyMaterial( "materials/editor/minigolf_wall/minigolf_water.vmat" )]
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

		public override void StartTouch( Entity other )
		{
			if ( other is not Ball ball )
				return;

			if ( !IsServer )
				return;

			ball.InWater = true;

			using ( Prediction.Off() )
			{
				Sound.FromWorld( "minigolf.ball_in_water", ball.Position );
				Particles.Create( "particles/gameplay/ball_water_splash/ball_water_splash.vpcf", ball.Position );

				Action task = async () =>
				{
					await Task.DelaySeconds( 2 );

					if ( !other.IsValid() )
						return;

				// TODO: check if other is still in water
				if ( other is Ball ball )
						Game.Current.BallOutOfBounds( ball, Game.OutOfBoundsType.Water );
				};
				task.Invoke();
			}
		}
	}
}
