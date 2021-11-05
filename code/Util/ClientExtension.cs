using Sandbox;
using System.Linq;

namespace Minigolf
{
	public static partial class ClientExtensions
	{
		public static Entity GetEntity( this Client self ) => Entity.FindByIndex( self.NetworkIdent );
		public static SteamID SteamID( this Client self ) => new( self.SteamId );
		public static bool IsBot( this Client self ) => self.SteamID().AccountType == SteamIDAccountType.AnonGameServer;
		public static bool IsHost( this Client self ) => Global.IsListenServer && self.NetworkIdent == 1;
		public static int GetPar( this Client self, int hole ) => self.GetInt( $"par_{hole}" );
		public static void AddPar( this Client self, int hole ) => self.AddInt( $"par_{hole}" );
		public static int GetPar( this Client self ) => self.GetPar( Game.Current.Course._currentHole );
		public static void AddPar( this Client self ) => self.AddPar( Game.Current.Course._currentHole );

		public static int GetTotalPar( this Client self )
		{
			int total = 0;

			// This isn't great, but we don't have access to the underlying list... ?
			for ( int i = 0; i < 24; i++ )
			{
				total += self.GetPar( i );
			}

			return total;
		}
	}
}
