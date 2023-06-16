using Facepunch.Minigolf.Entities.Desktop;

namespace Facepunch.Minigolf.Entities;

public partial class Ball : ModelEntity
{
	[AttributeUsage( AttributeTargets.Property )]
	public class ComponentDependency : Attribute
	{
	}

	public class Component : EntityComponent<Ball>
	{
		public virtual void Simulate( IClient cl ) { }
		public virtual void FrameSimulate( IClient cl ) { }
		public virtual void SharedSimulate( IClient cl, bool frame ) { }
		public virtual void BuildInput() { }

		protected Ball Ball => Entity;

		protected Vector3 Position
		{
			get => Entity.Position;
			set => Entity.Position = value;
		}

		protected Vector3 Velocity
		{
			get => Entity.Velocity;
			set => Entity.Velocity = value;
		}

		protected Rotation Rotation
		{
			get => Entity.Rotation;
			set => Entity.Rotation = value;
		}

		protected Transform Transform => Entity.Transform;
	}

	public static readonly Model GolfBallModel = Model.Load( "models/golf_ball.vmdl" );
	[BindComponent] public FollowBallCamera Camera { get; }

	public IInputComponent Input { get; private set; }
	public IMovementComponent Movement { get; private set; }

	[Net, Predicted] public bool InWater { get; set; }
	[Net, Predicted] public bool InPlay { get; set; }
	[Net, Predicted] public bool Cupped { get; set; }

	private Glow _glow;
	private string _defaultSkin;

	public override void Spawn()
	{
		base.Spawn();

		_defaultSkin = GolfBallModel.Materials.FirstOrDefault().ResourcePath;

		Model = GolfBallModel;
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Transmit = TransmitType.Always;
		EnableTraceAndQueries = false;
		Predictable = true;

		Input = Components.Create<DesktopInputComponent>();
		
		Tags.Add( "golf_ball" );
	}

	public override void ClientSpawn()
	{
		Components.Create<FollowBallCamera>();

		base.ClientSpawn();

		_glow = Components.GetOrCreate<Glow>();

		_glow.Enabled = true;
		_glow.Width = 0.25f;
		_glow.Color = new Color( 0.0f, 0.0f, 0.0f, 0.0f );
		_glow.ObscuredColor = Color.Transparent;
		_glow.InsideObscuredColor = Color.Black.WithAlpha( 0.72f );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		foreach ( var component in Components.GetAll<Component>() )
		{
			component.Simulate( cl );
			component.SharedSimulate( cl, false );
		}
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		foreach ( var component in Components.GetAll<Component>() )
		{
			component.FrameSimulate( cl );
			component.SharedSimulate( cl, true );
		}
	}
}
