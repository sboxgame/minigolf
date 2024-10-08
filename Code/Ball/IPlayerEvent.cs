public interface IPlayerEvent : ISceneEvent<IPlayerEvent>
{
	void OnSpawned( Ball ball ) { }
}
