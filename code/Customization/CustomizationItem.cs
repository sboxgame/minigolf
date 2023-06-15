namespace Facepunch.Customization;

[GameResource( "Ball Customization Item", "ballitem", "", Icon = "texture" )]
public class CustomizationItem : GameResource
{
	public enum CategoryType
	{
		Hat,
		Skin,
		Trail
	}

	public CategoryType Category { get; set; }

	[Title( "Icon" ), ResourceType( "png" )]
	public string Icon { get; set; }

	[ResourceType( "vmdl" ), ShowIf( nameof( Category ), CategoryType.Hat )]
	public string HatModel { get; set; }

	[ResourceType( "vmat" ), ShowIf( nameof( Category ), CategoryType.Skin )]
	public string SkinTexture { get; set; }

	[ResourceType( "vpcf" ), ShowIf( nameof( Category ), CategoryType.Trail )]
	public string TrailParticle { get; set; }

	public static CustomizationItem Find( string resourceName )
	{
		return ResourceLibrary.GetAll<CustomizationItem>().FirstOrDefault( x => x.ResourceName == resourceName );
	}
}
