public sealed class HoleStart : Component
{
	/// <summary>
	/// The starting point of a hole
	/// </summary>
	public Hole Hole => GetComponentInParent<Hole>();
}
