public sealed class Course : Component
{
	public static Course Current { get; private set; }

	public string Title => Scene.Title;
	public string Description => Scene.Description;
	public IEnumerable<Hole> Holes => Scene.GetAllComponents<Hole>();
	public int HoleCount => Holes.Count();

	/// <summary>
	/// A special name for the course, we use this to store stats.
	/// </summary>
	[Property] 
	public string Ident { get; set; }

	protected override void OnStart()
	{
		Current = this;
	}
}
