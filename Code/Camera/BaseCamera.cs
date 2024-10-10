namespace Facepunch.Minigolf;

public partial class BaseCamera : Component
{
	/// <summary>
	/// The ball
	/// </summary>
	[Property]
	public Ball Ball { get; set; }

	/// <summary>
	/// The camera component
	/// </summary>
	[Property]
	public CameraComponent Camera { get; set; }

	private readonly List<ViewBlocker> _viewBlockers = new();

	public void UpdateViewBlockers()
	{
		foreach ( var ent in _viewBlockers )
		{
			ent.BlockingView = false;
		}

		_viewBlockers.Clear();

		var traces = Scene.Trace.Ray( Camera.WorldPosition, Ball.Local.WorldPosition )
			.RunAll();

		if ( traces == null )
			return;

		foreach ( var tr in traces )
		{
			if ( tr.Component.GetComponent<ViewBlocker>() is { } blocker )
			{
				blocker.BlockingView = true;
				_viewBlockers.Add( blocker );
			}
		}
	}

	public virtual void OnCameraActive()
	{
	}

	public virtual void OnCameraInactive()
	{
	}

	public virtual void Tick()
	{
	}
}
