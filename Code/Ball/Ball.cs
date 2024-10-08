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
	}
}
