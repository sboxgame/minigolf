namespace Facepunch.Minigolf;

public partial class HoleNumber : Component
{
	/// <summary>
	/// The text renderer
	/// </summary>
	[Property] 
	public TextRenderer Renderer { get; set; }

	/// <summary>
	/// What hole?
	/// </summary>
	public Hole Hole => GetComponentInParent<Hole>();

	protected override void OnStart()
	{
		var number = Hole.Number.ToString( "X2" );
		Renderer.Text = $"{number}  {number}";
	}
}
