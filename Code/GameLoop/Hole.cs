public sealed class Hole : Component
{
	/// <summary>
	/// The hole number
	/// </summary>
	[Property]
	public int Number { get; set; } = 1;

	[Property]
	public int Par { get; set; } = 3;

	public override string ToString()
	{
		return $"Hole {Number}, Par {Par}";
	}
}
