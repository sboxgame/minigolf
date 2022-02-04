using Sandbox;
using System.Text.Json;
using System.Threading.Tasks;

namespace Facepunch.Customization;

public static class Customization
{

	private static CustomizationConfig loadedConfig;
	private static ulong crc;
	private const string filePath = "config/customization.json";

	public static CustomizationConfig Config
	{
		get
		{
			if ( loadedConfig == null )
				loadedConfig = LoadConfig().Result;
			return loadedConfig;
		}
	}

	public static async Task<bool> IsDirty()
	{
		var newcrc = await FileSystem.Mounted.GetCRC( filePath );
		return newcrc != crc;
	}

	public static async Task<CustomizationConfig> LoadConfig()
	{
		if ( !FileSystem.Mounted.FileExists( filePath ) ) return new();

		var json = FileSystem.Mounted.ReadAllText( filePath );
		loadedConfig = JsonSerializer.Deserialize<CustomizationConfig>( json );
		crc = await FileSystem.Mounted.GetCRC( filePath );

		return loadedConfig;
	}

}
