using System.Collections.Generic;
using Sandbox;

namespace Minigolf
{
	/// <summary>
	/// Bounds of a hole.
	/// </summary>
	[Library( "minigolf_hole_bounds" )]
	[Hammer.Solid]
	[Hammer.AutoApplyMaterial]
	public partial class HoleBounds : ModelEntity
	{
		/// <summary>
		/// Which hole this hole is on.
		/// </summary>
		[Property]
		public int HoleNumber { get; set; }

		public IEnumerable<Ball> TouchingBalls => touchingBalls;
		private readonly List<Ball> touchingBalls = new();

		public override void StartTouch(Entity other)
		{
			base.StartTouch(other);

			if (other is Ball ball)
				AddTouchingBall( ball );
		}

		public override void EndTouch(Entity other)
		{
			base.EndTouch(other);

			if ( other is not Ball )
				return;

			var ball = other as Ball;

			if (touchingBalls.Contains(ball))
				touchingBalls.Remove(ball);
		}

		protected void AddTouchingBall( Ball ball )
		{
			if (!ball.IsValid())
				return;

			if (!touchingBalls.Contains(ball))
				touchingBalls.Add(ball);
		}
	}
}
