using Sandbox;
using System;

namespace Minigolf
{
	public partial class Ball
	{
		static readonly string[][] SwingSounds = new string[][] {
			new string[] {
				new("minigolf.swing_supersoft_01"),
				new("minigolf.swing_supersoft_02"),
				new("minigolf.swing_supersoft_03"),
			},
			new string[] {
				new("minigolf.swing_soft_01"),
				new("minigolf.swing_soft_02"),
				new("minigolf.swing_soft_03"),
			},
			new string[] {
				new("minigolf.swing_medium_01"),
				new("minigolf.swing_medium_02"),
				new("minigolf.swing_medium_03"),
			},
			new string[] {
				new("minigolf.swing_hard_01"),
				new("minigolf.swing_hard_02"),
				new("minigolf.swing_hard_03"),
			},
		};

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

			var sound = SwingSounds[(int)MathF.Ceiling(power / 25)][Rand.Int(0, 2)];
			Sound.FromWorld(sound, Position);

			PhysicsBody.Velocity = direction * power * PowerMultiplier;
			PhysicsBody.AngularVelocity = 0;
			PhysicsBody.Wake();
		}
	}
}
