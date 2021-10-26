using Sandbox;

namespace Minigolf
{
	public partial class Game
	{
		void AddToPrecache()
		{
			// Particles don't have a smart way to precache like models and materials do
			Precache.Add( "particles/gameplay/ball_circle/ball_circle.vpcf" );
			Precache.Add( "particles/gameplay/ball_hit/ball_hit.vpcf" );
			Precache.Add( "particles/gameplay/ball_hit/ball_hit_smoke.vpcf" );
			Precache.Add( "particles/gameplay/ball_trail/ball_trail.vpcf" );
			Precache.Add( "particles/gameplay/ball_trail/ball_trail_a.vpcf" );
			Precache.Add( "particles/gameplay/hole_effect/confetti.vpcf" );
			Precache.Add( "particles/gameplay/hole_effect/confetti_a.vpcf" );
			Precache.Add( "particles/gameplay/power_arrow/power_arrow.vpcf" );
		}
	}
}
