namespace Facepunch.Minigolf;

public partial class GameManager : Component, Component.INetworkListener,
	ISceneStartup, IGameEvent
{
	void ISceneStartup.OnHostInitialize()
	{
		// If we're not hosting a lobby, start hosting one
		// so that people can join this game.
		Networking.CreateLobby();

		var holes = Scene.GetAllComponents<Hole>().ToArray();
		if ( holes.Length > 0 )
		{
			var firstHole = holes
				.OrderBy( x => x.Number )
				.First();

			CurrentHole = firstHole;

			Log.Info( $"Assigning first hole {CurrentHole}" );
		}
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
		var holes = Scene.GetAllComponents<Hole>().ToArray();
		if ( holes.Length > 0 )
		{
			var transform = holes
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

	/// <summary>
	/// Called when the ball changes play mode
	/// </summary>
	/// <param name="ball"></param>
	/// <param name="inPlay"></param>
	void IGameEvent.BallInPlay( Ball ball, bool inPlay )
	{
		Log.Info( $"{ball} changed to {inPlay}" );
	}

	/// <summary>
	/// Called when the ball gets hit
	/// </summary>
	/// <param name="ball"></param>
	void IGameEvent.BallStroke( Ball ball )
	{
		Log.Info( $"{ball} struck" );
	}

	void IGameEvent.OnGoal( Ball ball, HoleGoal goal )
	{
		Log.Info( $"{ball} hit {goal}" );

		ball.IsCupped = true;
		Sound.Play( "minigolf.sink_into_cup", goal.WorldPosition );
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			CheckBoundsTimes();
		}
	}
}
