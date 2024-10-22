/// <summary>
/// A definition component for a course. This is most useful in a course's scene.
/// It gives information about a course, how many holes, the ident [for stats], title, description, etc...
/// </summary>
public sealed class Course : Component, ISceneMetadata
{
	/// <summary>
	/// A special name for the course, we use this to store stats.
	/// </summary>
	[Property] 
	public string Ident { get; set; }

	[Property, Range( 0, 5 )]
	public int Stars { get; set; }

	Dictionary<string, string> ISceneMetadata.GetMetadata()
	{
		var d = new Dictionary<string, string>();
		d["Ident"] = Ident;
		d["Stars"] = Stars.ToString();
		return d;
	}
}
