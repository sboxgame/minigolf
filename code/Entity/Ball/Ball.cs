using Sandbox;

namespace Minigolf
{
	public partial class Ball : ModelEntity
	{
		[ServerVar( "minigolf_ball_debug" )]
		public static bool Debug { get; set; } = false;
		[Net] public bool InPlay { get; set; } = false;
		[Net] public bool Cupped { get; set; } = false;
		[Net] public Angles Direction { get; set; }

		static readonly Model Model = Model.Load( "models/golf_ball.vmdl" );

		public override void Spawn()
		{
			base.Spawn();

			SetModel( Model );
			SetupPhysicsFromModel( PhysicsMotionType.Static, false );

			CollisionGroup = CollisionGroup.Debris;
			EnableTraceAndQueries = false;

			Transmit = TransmitType.Always;

			Predictable = false;

			Tags.Add( "golf_ball" );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			CreateParticles();
		}

		public void Cup( bool holeInOne = false )
		{
			if ( Cupped ) return;

			Cupped = true;

			var sound = PlaySound( "minigolf.sink_into_cup" );
			sound.SetVolume( 1.0f );
			sound.SetPitch( Rand.Float(0.75f, 1.25f) );
		}

		public void ResetPosition( Vector3 position, Angles direction )
		{
			// Reset all velocity
			PhysicsBody.Velocity = Vector3.Zero;
			PhysicsBody.AngularVelocity = Vector3.Zero;
			PhysicsBody.ClearForces();
			PhysicsBody.ClearTorques();

			Position = position;
			PhysicsBody.Position = position;
			ResetInterpolation();

			InPlay = false;
			Cupped = false;

			Direction = direction;

			// Tell the player we reset the ball
			PlayerResetPosition( To.Single(this), position, direction );
		}

		[ClientRpc]
		protected void PlayerResetPosition( Vector3 position, Angles angles )
		{
			Game.Current.BallCamera.Angles = new (14, angles.yaw, 0);
		}
	}
}
