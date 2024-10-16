using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public record CourseInfo( string Ident, int Stars );

/// <summary>
/// A definition component for a course. This is most useful in a course's scene.
/// It gives information about a course, how many holes, the ident [for stats], title, description, etc...
/// </summary>
public sealed class Course : Component
{
	/// <summary>
	/// Singleton for the course
	/// </summary>
	private static Course Current { get; set; }

	public static CourseInfo CurrentInfo => new CourseInfo( Current.Ident, Current.Stars );

	/// <summary>
	/// A special name for the course, we use this to store stats.
	/// </summary>
	[Property] 
	public string Ident { get; set; }

	[Property, Range( 0, 5 )]
	public int Stars { get; set; }

	protected override void OnStart()
	{
		Current = this;
	}

	/// <summary>
	/// Looks for a Course component in a scene file and gives it to you
	/// </summary>
	/// <param name="scene"></param>
	/// <returns></returns>
	public static CourseInfo GetCourseInfo( SceneFile scene )
	{
		foreach ( var go in scene.GameObjects )
		{
			if ( go.TryGetPropertyValue( "Components", out var componentsNode ) && componentsNode is JsonArray components )
			{
				foreach ( var component in components.Select( x => x.AsObject() ) )
				{
					if ( !component.TryGetPropertyValue( "__type", out var typeNode )
						|| !string.Equals( typeNode?.GetValue<string>(), typeof( Course ).FullName, StringComparison.OrdinalIgnoreCase ) )
					{
						continue;
					}

					if ( !component.TryGetPropertyValue( "Ident", out var identNode ) )
					{
						continue;
					}

					var course = Json.FromNode<CourseInfo>( component );

					return course;
				}
			}
		}

		return null;
	}
}
