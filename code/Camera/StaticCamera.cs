using Sandbox;

namespace Facepunch.Minigolf;

/// <summary>
/// A simple camera component that turns any entity into a camera itself.
/// </summary>
public class StaticCamera : BaseCamera
{
	public Vector3 Position;
	public Rotation Rotation;
	public float FOV = 80;

    public override void Update()
	{
		Camera.Position = Position;
		Camera.Rotation = Rotation;
		Camera.FieldOfView = FOV;
	}
}
