
/// <summary>
/// A definition component for a course. This is most useful in a course's scene.
/// It gives information about a course, how many holes, the ident [for stats], title, description, etc...
/// </summary>
public sealed class Course : Component
{
	/// <summary>
	/// Singleton for the course
	/// </summary>
	public static Course Current { get; private set; }

	/// <summary>
	/// What's the Course called?
	/// </summary>
	public string Title => Scene.Title;

	/// <summary>
	/// A short description for the Course
	/// </summary>
	public string Description => Scene.Description;
	
	/// <summary>
	/// A collection of all of the holes
	/// </summary>
	public IEnumerable<Hole> Holes => Scene.GetAllComponents<Hole>();
	
	/// <summary>
	/// How many holes does this course have
	/// </summary>
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
