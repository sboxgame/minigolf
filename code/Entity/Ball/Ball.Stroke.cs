using Sandbox;
using System;

namespace Minigolf
{
	public partial class Ball
	{
		/// <summary>
		/// Applies a velocity to the ball's PhysicsBody
		/// This does not check if the player is allowed.
		/// </summary>
		/// <param name="direction">Normalized </param>
		/// <param name="power">Normalized power 0-1</param>
		public void Stroke( Vector3 direction, float power )
		{
			// TODO: Run a ReadyToShoot variable or something
			// if ( Cupped || (!UnlimitedWhacks && Moving) )
			// 	return;

			InPlay = true;

			direction = direction.Normal.WithZ(0);
			power = Math.Clamp( power, 0, 1 );

			// gradient the power, smaller shots have less power
			// y = 2.78(0.5x + 0.1)^2
			power = 2.78f * MathF.Pow( 0.5f * power + 0.1f, 2.0f );

			var sound = "minigolf.swing" + Rand.Int( 1, 3 );
			Sound.FromWorld( sound, Position ).SetVolume( 1.0f + power ).SetPitch( Rand.Float( 0.8f, 1.2f ) );

			Direction = direction.EulerAngles;

			Velocity = direction * power * PowerMultiplier;
			PhysicsBody.Velocity = direction * power * PowerMultiplier;
			PhysicsBody.AngularVelocity = 0;
			PhysicsBody.Wake();
		}
	}
}
