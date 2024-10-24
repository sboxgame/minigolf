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
	/// The cosmetic's category for UI
	/// </summary>
	[Property]
	public string Category { get; set; }

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

	/// <summary>
	/// We'll apply a trail if it exists
	/// </summary>
	[Property]
	public ParticleSystem Trail { get; set; }

	/// <summary>
	/// What achievements do we need unlocked for this?
	/// </summary>
	[Property]
	public List<string> RequiredAchievements { get; set; } = new()
	{
	};

	/// <summary>
	/// Accessor to see if an achievement is unlocked
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	private bool IsAchievementUnlocked( string str )
	{
		return Sandbox.Services.Achievements.All.FirstOrDefault( x => x.Name == str && x.IsUnlocked ) is not null;
	}

	/// <summary>
	/// Are we allowed to equip this?
	/// </summary>
	/// <returns></returns>
	public bool CanEquip()
	{
		if ( RequiredAchievements is not null )
		{
			foreach ( var name in RequiredAchievements )
			{
				if ( !IsAchievementUnlocked( name ) ) return false;
			}
		}

		return true;
	}
}
