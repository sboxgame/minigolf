namespace Facepunch.Minigolf.Entities;

public interface IMovementComponent
{
	public Vector3 LastPosition { get; }
	public Angles LastAngles { get; }

	public void ResetPosition( Vector3 position, Angles angles );
}
