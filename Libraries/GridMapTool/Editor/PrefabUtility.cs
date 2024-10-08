using Sandbox.Diagnostics;
using System.Text.Json.Nodes;

namespace Editor;

public static class PrefabUtility
{
	public static GameObject CreateGameObject( PrefabFile prefabFile, GameObject parent = null )
	{
		Assert.NotNull( Game.ActiveScene, "No Active Scene" );

		JsonObject json = prefabFile.RootObject;

		//if ( template is PrefabScene prefabScene && prefabScene.Source is PrefabFile prefabFile )
		//{
		//	json = prefabFile.RootObject;
		//}
		//else
		//{
		//	json = template.Serialize();
		//}

		MakeGameObjectsUnique( json );

		var go = new GameObject();
		go.Deserialize( json );
		go.SetPrefabSource( prefabFile.ResourcePath );

		if ( parent != null )
		{
			go.Parent = parent;
		}

		return go;
	}

	/// <summary>
	/// Find all "Id" guids, and replace them with new Guids.
	/// This is used to make GameObject serializations unique, so when
	/// you duplicate stuff, it copies over uniquely and keeps associations.
	/// </summary>
	public static void MakeGameObjectsUnique( JsonObject json )
	{
		Dictionary<Guid, Guid> translate = new();

		//
		// Find all guids with "Id" as their name. Add them to translate 
		// with a new target value.
		//
		Sandbox.Json.WalkJsonTree( json, ( k, v ) =>
		{
			if ( k != "Id" ) return v;

			if ( v.TryGetValue<Guid>( out var guid ) )
			{
				translate[guid] = Guid.NewGuid();
			}

			return v;
		} );

		//
		// Find every guid and translate them, but only if they're in our
		// guid dictionary.
		//
		Sandbox.Json.WalkJsonTree( json, ( k, v ) =>
		{
			if ( !v.TryGetValue<Guid>( out var guid ) ) return v;
			if ( !translate.TryGetValue( guid, out var updatedGuid ) ) return v;

			return updatedGuid;
		} );
	}
}
