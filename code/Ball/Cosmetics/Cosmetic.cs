namespace Facepunch.Minigolf;

public partial class Cosmetic : Component, ISceneMetadata
{
	/// <summary>
	/// Fetch all the cosmetics we have from their prefabs, and return their root GameObject.
	/// </summary>
	public static IEnumerable<GameObject> AllPrefabs
	{
		get
		{
			return ResourceLibrary.GetAll<PrefabFile>()
				.Where( x => x.GetMetadata( "IsCosmetic", "false" ).Equals( "true" ) )
				.Select( x => GameObject.GetPrefab( x.ResourcePath ) );
		}
	}

	/// <summary>
	/// Parse the <see cref="Cosmetic"/> component from their cached prefab.
	/// </summary>
	public static IEnumerable<Cosmetic> All => AllPrefabs.Select( x => x.GetComponent<Cosmetic>() );

	/// <summary>
	/// Mark the cosmetics as a cosmetic so we can grab them from a list.
	/// </summary>
	/// <returns></returns>
	Dictionary<string, string> ISceneMetadata.GetMetadata()
	{
		return new()
		{
			{ "IsCosmetic", "true" }
		};
	}

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
