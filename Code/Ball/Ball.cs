/// <summary>
/// A ball
/// </summary>
public sealed class Ball : Component
{
	/// <summary>
	/// The local player's ball
	/// </summary>
	public static Ball Local { get; set; }

	/// <summary>
	/// The ball's Rigidbody
	/// </summary>
	[RequireComponent]
	public Rigidbody Rigidbody { get; set; }

	/// <summary>
	/// Controls for the ball
	/// </summary>
	[RequireComponent]
	public BallController Controller { get; set; }

	/// <summary>
	/// Which sound do we play when hitting the ball
	/// </summary>
	[Property]
	public SoundEvent SwingSound { get; set; }

	/// <summary>
	/// The ball's trail
	/// </summary>
	[Property]
	public GameObject Trail { get; set; }

	/// <summary>
	/// How powerful should a stroke be
	/// </summary>
	[Property]
	public float PowerMultiplier { get; set; } = 2500.0f;

	/// <summary>
	/// The player's last position when they hit the ball
	/// </summary>
	public Vector3 LastPosition { get; set; }

	/// <summary>
	/// The player's last angles when they hit the ball
	/// </summary>
	public Angles LastAngles { get; set; }

	/// <summary>
	/// Is this ball cupped?
	/// </summary>
	[HostSync]
	public bool IsCupped { get; set; }

	/// <summary>
	/// A tracking history of the pars, we might want to move this though
	/// </summary>
	[Sync]
	private NetDictionary<Hole, int> Pars { get; set; } = new();

	/// <summary>
	/// Get a player's par for a current hole
	/// </summary>
	/// <param name="hole"></param>
	/// <returns></returns>
	public int GetPar( Hole hole )
	{
		if ( Pars.TryGetValue( hole, out var par ) )
		{
			return par;
		}

		return 0;
	}

	/// <summary>
	/// Get the player's current par for the current hole
	/// </summary>
	/// <returns></returns>
	public int GetCurrentPar()
	{
		return GetPar( Facepunch.Minigolf.GameManager.Instance.CurrentHole );
	}

	/// <summary>
	/// Sets the player's par on a specific hole
	/// </summary>
	/// <param name="hole"></param>
	/// <param name="par"></param>
	public void SetPar( Hole hole, int par )
	{
		Pars[hole] = par;
	}

	/// <summary>
	/// Increments the player's par on a specific hole
	/// </summary>
	/// <param name="hole"></param>
	public void IncrementPar( Hole hole )
	{
		if ( !Pars.TryGetValue( hole, out var par ) )
		{
			Pars[hole] = 0;
		}

		Pars[hole]++;
	}

	/// <summary>
	/// Hit your ball
	/// </summary>
	/// <param name="yaw"></param>
	/// <param name="power"></param>
	public void Stroke( float yaw, float power )
	{
		var direction = Angles.AngleVector( new Angles( 0, yaw, 0 ) );
		direction = direction.Normal.WithZ( 0 );

		// Gradient the power, smaller shots have less power
		power = 2.78f * MathF.Pow( 0.5f * power + 0.1f, 2.0f );

		var force = direction * power * PowerMultiplier;
		Rigidbody.ApplyForce( force );
		GameObject.PlaySound( SwingSound );

		// Store last known positions
		LastPosition = WorldPosition;
		LastAngles = new Angles( 0, yaw, 0 );

		// Increment par if we're in a hole
		if ( Facepunch.Minigolf.GameManager.Instance.CurrentHole is { } hole )
		{
			IncrementPar( hole );
		}
	}

	protected override void OnStart()
	{
		if ( !IsProxy ) Local = this;
	}

	/// <summary>
	/// Reset the player's position to the last stroke spot
	/// </summary>
	public void ResetPosition()
	{
		ResetPosition( LastPosition, LastAngles );
	}

	public void ResetPosition( Vector3 position, Angles angles )
	{
		Trail.Enabled = false;

		// Move the player
		WorldPosition = position;

		// Halt the player
		Rigidbody.Velocity = 0;
		Rigidbody.AngularVelocity = 0;

		// This is the new info
		LastPosition = position;
		LastAngles = angles;

		Trail.Enabled = true;
	}
}
