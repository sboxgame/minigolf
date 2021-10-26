using Sandbox;

namespace Minigolf
{
	/// <summary>
	/// A generic brush/mesh that has customizable reflectivity.
	/// </summary>
	[Library( "minigolf_wall" )]
	[Hammer.Solid]
	[Hammer.AutoApplyMaterial( "materials/editor/minigolf_wall/minigolf_wall.vmat" )]
	[Hammer.PhysicsTypeOverride( Hammer.PhysicsTypeOverrideAttribute.PhysicsTypeOverride.Mesh )]
	public partial class Wall : ModelEntity
	{
		/// <summary>
		/// If checked, the ball will bounce off this surface at the defined multiplier.
		/// </summary>
		[Property]
		public bool Reflect { get; set; } = true;

		/// <summary>
		/// How much the wall will reflect
		/// </summary>
		[Property]
		public float ReflectMultiplier { get; set; } = 1;

		//  surface_property_override(surface_properties) : "Surface Property Override" : "" : "Overrides the default surface property."

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );

			// TODO: Property this
			EnableDrawing = false;

			// Custom surface property with 0 friction, 0 elasticity.
			PhysicsGroup.SetSurface( "minigolf.side" );

			Transmit = TransmitType.Always;
		}
	}
}
