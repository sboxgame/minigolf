public sealed class Hole : Component
{
	/// <summary>
	/// The hole number
	/// </summary>
	[Property]
	public int Number { get; set; } = 1;

	public override string ToString()
	{
		return $"Hole {Number}";
	}
}
