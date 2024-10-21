using Facepunch.Minigolf.UI;

public sealed class MainMenuCamera : Component, IMainMenuEvents
{
	/// <summary>
	/// The default marker if we don't have a specified page
	/// </summary>
	[Property]
	public GameObject DefaultMarker { get; set; }

	/// <summary>
	/// The marker for the customization page
	/// </summary>
	[Property]
	public GameObject CustomizationMarker { get; set; }

	/// <summary>
	/// How fast do we lerp page to page
	/// </summary>
	[Property]
	public float LerpSpeed { get; set; } = 10f;

	/// <summary>
	/// The last stored main menu url
	/// </summary>
	private string Url { get; set; } = "/";

	/// <summary>
	/// Chooses which marker to look at
	/// </summary>
	/// <returns></returns>
	public GameObject GetMarker()
	{
		if ( Url == "/avatar" ) return CustomizationMarker;
		return DefaultMarker;
	}

	void IMainMenuEvents.OnNavigated( string url )
	{
		Url = url;
	}

	protected override void OnUpdate()
	{
		var marker = GetMarker();
		var spd = Time.Delta * LerpSpeed;

		WorldPosition = WorldPosition.LerpTo( marker.WorldPosition, spd );
		WorldRotation = Rotation.Lerp( WorldRotation, marker.WorldRotation, spd );
	}
}
