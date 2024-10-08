public sealed class HoleGoal : Component
{
	/// <summary>
	/// Which hole is this a goal for?
	/// </summary>
	[Property] 
	public Hole Hole { get; set; }
}
