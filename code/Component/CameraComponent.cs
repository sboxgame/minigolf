using Sandbox;

namespace Facepunch.Minigolf;

public partial class CameraComponent : EntityComponent
{
	public float AspectRatio { get; set; } = 1.0f;
	public float FieldOfView { get; set; } = 80.0f;
	public float ZFar { get; set; } = 10.0f;
	public float ZNear { get; set; } = 80000.0f;

	public void BuildCamera( ref CameraSetup camSetup )
	{
		camSetup.Position = Entity.Position;
		camSetup.Rotation = Entity.Rotation;
		camSetup.Aspect = AspectRatio;
		camSetup.ZFar = ZFar;
		camSetup.ZNear = ZNear;
	}
}
