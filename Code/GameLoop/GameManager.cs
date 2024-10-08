using System.Linq;

public sealed class GameManager : Component, Component.INetworkListener, ISceneStartup
{
	void ISceneStartup.OnHostInitialize()
	{
		// If we're not hosting a lobby, start hosting one
		// so that people can join this game.
		Networking.CreateLobby();
	}

	void INetworkListener.OnActive( Connection channel )
	{
		SpawnPlayerForConnection( channel );
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		var spawnPoints = Scene.GetAllComponents<Hole>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			var transform = spawnPoints
				.OrderBy( x => x.Number )
				.First()
				.WorldTransform;

			return transform.WithPosition( transform.Position + Vector3.Up * 16f );
		}

		//
		// Failing that, spawn where we are
		//
		return global::Transform.Zero;
	}

	public void SpawnPlayerForConnection( Connection channel )
	{
		// Find a spawn location for this player
		var startLocation = FindSpawnLocation().WithScale( 1 );

		// Spawn this object and make the client the owner
		var playerGo = GameObject.Clone( "prefabs/ball.prefab", new CloneConfig { Name = $"Player - {channel.DisplayName}", StartEnabled = true, Transform = startLocation } );
		var player = playerGo.Components.Get<Ball>( true );
		playerGo.NetworkSpawn( channel );

		IPlayerEvent.Post( x => x.OnSpawned( player ) );
	}
}
