using Sandbox;

namespace Minigolf
{
	[Library( "minigolf_flag_base", Description = "Minigolf Flag Base" )]
	[Hammer.EditorModel( "models/minigolf_flag.vmdl" )]
	public partial class FlagBase : ModelEntity
	{
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/minigolf_flag.vmdl" );

			MoveType = MoveType.None;
			CollisionGroup = CollisionGroup.Never;
			PhysicsEnabled = false;
			UsePhysicsCollision = false;

			Transmit = TransmitType.Always;
		}
	}
}
