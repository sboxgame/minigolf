public sealed class Hole : Component
{
	/// <summary>
	/// The hole number
	/// </summary>
	[Property]
	public int Number { get; set; } = 1;

	/// <summary>
	/// The par
	/// </summary>
	[Property]
	public int Par { get; set; } = 3;

	/// <summary>
	/// The starting point for this hole
	/// </summary>
	public HoleStart Start => GetComponentInChildren<HoleStart>();

	/// <summary>
	/// The goal for this hole
	/// </summary>
	public HoleGoal Goal => GetComponentInChildren<HoleGoal>();

	public override string ToString()
	{
		return $"Hole {Number}, Par {Par}";
	}
}
