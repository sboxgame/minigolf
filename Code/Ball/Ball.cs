/// <summary>
/// A ball
/// </summary>
public sealed class Ball : Component
{
	[RequireComponent]
	public Rigidbody Rigidbody { get; set; }
}
