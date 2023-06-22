[System.AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
public sealed class MenuAttribute : Attribute
{
	public string Path { get; private set; }
	public string Name { get; private set; }
	public string Description { get; private set; }
	public int Order { get; private set; }

	public MenuAttribute( string path, string name, string description, int order = -1 )
	{
		Path = path;
		Name = name;
		Description = description;
		Order = order;
	}
}
