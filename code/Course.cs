using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Minigolf
{
    public partial class Course : BaseNetworkable
	{
        public string Name { get; set; } = "Default";
		public string Description { get; set; } = "Default Description";
		public Dictionary<int, HoleInfo> Holes { get; set; }

		[Net] public int _currentHole { get; set; } = 1;
		public HoleInfo CurrentHole
		{
			get
			{
				// Sanity check for networked
				if ( Holes == null || !Holes.ContainsKey(_currentHole) )
					return new HoleInfo { Number = _currentHole, Name = "oops" };

				return Holes[_currentHole];
			}
		}

		// Register for events
		public Course() => Event.Register( this );
		~Course() => Event.Unregister( this );

		[Event.Entity.PostSpawn]
		public void Load()
		{
			// TODO: Instead of loading the clientside this class could be a INetworkSerializable?
			Holes = new Dictionary<int, HoleInfo>();

			foreach ( var hole in Entity.All.OfType<BallSpawnpoint>().OrderBy( ent => ent.HoleNumber ) )
			{
				var goal = Entity.All.OfType<HoleGoal>().Where( x => x.HoleNumber == hole.HoleNumber ).First();

				if ( goal == null )
				{
					Log.Error( $"No ball goal found for [Hole {hole.HoleNumber}]" );
					continue;
				}

				Holes.Add( hole.HoleNumber, new HoleInfo()
					{
						Number = hole.HoleNumber,
						Name = hole.HoleName,
						Par = hole.HolePar,
						SpawnPosition = hole.Position,
						SpawnAngles = hole.Rotation.Angles(),
						GoalPosition = goal.Position,
					}
				);
			}
		}

        public void NextHole()
        {
			var matchedHoles = Holes.Where( x => x.Key > _currentHole )
				.OrderBy( x => x.Key );

			// No more holes to advance to? Return early.
			// This should be checked before calling this function.
			if ( !matchedHoles.Any() )
				return;

			var nextHole = matchedHoles.First();

			_currentHole = nextHole.Key;

			// Announce to all clients that the course has advanced.
			// clientNextHole( To.Everyone, _currentHole );

			// Run an event so we can pick this up anywhere in the code base.
			Event.Run( "minigolf.advanced_hole", _currentHole );
        }
	}

	public struct HoleInfo
    {
		public int Number;
		public string Name;
		public int Par;
		public Vector3 SpawnPosition;
		public Angles SpawnAngles;
		public Vector3 GoalPosition;
    }
}
