/// <summary>
/// A ball
/// </summary>
public sealed class Ball : Component
{
	[RequireComponent]
	public Rigidbody Rigidbody { get; set; }

	[RequireComponent]
	public BallController Controller { get; set; }

	[Property]
	public SoundEvent SwingSound { get; set; }

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
		// Move the player
		WorldPosition = position;

		// Halt the player
		Rigidbody.Velocity = 0;
		Rigidbody.AngularVelocity = 0;

		// This is the new info
		LastPosition = position;
		LastAngles = angles;
	}
}
