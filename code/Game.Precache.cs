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

			// Ideally these precache from static SoundEvents but those don't work yet
			Precache.Add( "sounds/minigolf.sink_into_cup.vsnd" );
			Precache.Add( "sounds/impact/minigolf.ball_in_water.vsnd" );
			Precache.Add( "sounds/swing/minigolf.putt_hard.vsnd" );
			Precache.Add( "sounds/swing/minigolf.putt_into_hole.vsnd" );
			Precache.Add( "sounds/swing/minigolf.putt_into_hole2.vsnd" );
			Precache.Add( "sounds/swing/minigolf.putt1.vsnd" );
			Precache.Add( "sounds/swing/minigolf.rolls_in_hole.vsnd" );
			Precache.Add( "sounds/swing/minigolf.swing1.vsnd" );
			Precache.Add( "sounds/swing/minigolf.swing2.vsnd" );
			Precache.Add( "sounds/swing/minigolf.swing3.vsnd" );
			Precache.Add( "sounds/swing/minigolf.swing4.vsnd" );
			Precache.Add( "sounds/swing/minigolf.swing5.vsnd" );
			Precache.Add( "sounds/ui/minigolf.award.vsnd" );
		}
	}
}
