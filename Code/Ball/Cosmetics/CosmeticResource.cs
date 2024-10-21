namespace Facepunch.Minigolf;

[GameResource( "Minigolf/Cosmetic", "csmtc", "A cosmetic for Minigolf" )]
public partial class CosmeticResource : GameResource
{
	/// <summary>
	/// The cosmetic's name
	/// </summary>
	[Property]
	public string Title { get; set; }

	/// <summary>
	/// A short description for this cosmetic
	/// </summary>
	[Property]
	public string Description { get; set; }

	/// <summary>
	/// The prefab we'll spawn on the ball 
	/// </summary>
	[Property]
	public GameObject Prefab { get; set; }

	/// <summary>
	/// We'll apply a skin if it exists
	/// </summary>
	[Property]
	public Material Skin { get; set; }
}
