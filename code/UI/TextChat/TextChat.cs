namespace Facepunch.Minigolf.UI;

public partial class TextChat
{
	[ConCmd.Client( "golfchat_add", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string name, string message, string avatar = null )
	{
		Current?.AddEntry( name, message, avatar );

		// Only log clientside if we're not the listen server host
		if ( !Game.IsListenServer )
			Log.Info( $"{name}: {message}" );
	}

	[ConCmd.Client( "golfchat_addinfo", CanBeCalledFromServer = true )]
	public static void AddInformation( string message, string avatar = null )
	{
		Current?.AddEntry( null, message, avatar );
	}

	[ConCmd.Client( "golfchat_twitch", CanBeCalledFromServer = true )]
	public static void AddTwitch( string user, string message )
	{
		Current?.AddEntry( user, message, "ui/twitch.jpg" );
	}

	[ConCmd.Server( "golf_say" )]
	private static void Say( string message )
	{
		if ( ConsoleSystem.Caller == null )
			return;

		// todo - reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		Log.Info( $"{ConsoleSystem.Caller}: {message}" );
		AddChatEntry( To.Everyone, $"{ConsoleSystem.Caller.Name}", message, $"avatar:{ConsoleSystem.Caller.SteamId}" );
	}
}
