using Sandbox;

namespace Facepunch.Minigolf;

public static partial class ClientExtensions
{
	public static Entity GetEntity( this IClient self ) => Entity.FindByIndex( self.NetworkIdent );
	public static bool IsHost( this IClient self ) => Sandbox.Game.IsListenServer && self.NetworkIdent == 1;
	public static int GetPar( this IClient self, int hole ) => self.GetInt( $"par_{hole}" );
	public static void AddPar( this IClient self, int hole ) => self.AddInt( $"par_{hole}" );
	public static int GetPar( this IClient self ) => self.GetPar( MinigolfGame.Current.Course._currentHole );
	public static void AddPar( this IClient self ) => self.AddPar( MinigolfGame.Current.Course._currentHole );

	public static int GetTotalPar( this IClient self )
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
