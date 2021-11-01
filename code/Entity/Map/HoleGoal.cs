using Sandbox;

namespace Minigolf
{
	/// <summary>
	/// Minigolf hole goal trigger
	/// </summary>
	[Library( "minigolf_hole_goal" )]
	[Hammer.Solid]
	[Hammer.AutoApplyMaterial]
	[Hammer.EntityTool( "Hole goal", "Minigolf" )]
	[Hammer.VisGroup( Hammer.VisGroup.Trigger )]
	public partial class HoleGoal : ModelEntity
	{
		/// <summary>
		/// Which hole this hole is on.
		/// </summary>
		[Property, Net]
		public int HoleNumber { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel(PhysicsMotionType.Static);
			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = false;
			EnableTouch = true;
			EnableDrawing = false;
			Transmit = TransmitType.Always;
		}

		public override void StartTouch( Entity other )
		{
			if ( IsClient ) return;

			if ( other is not Ball ball )
				return;

			Game.Current.CupBall( ball, HoleNumber );
		}
	}
}
