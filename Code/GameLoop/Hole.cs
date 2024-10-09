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
	/// The goal
	/// </summary>
	public HoleGoal Goal => Scene.GetAllComponents<HoleGoal>().FirstOrDefault( x => x.Hole == this );

	public override string ToString()
	{
		return $"Hole {Number}, Par {Par}";
	}
}
