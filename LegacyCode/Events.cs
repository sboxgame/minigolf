namespace Facepunch.Minigolf;

public static partial class MinigolfEvent
{
	public const string NextHole = "game.next.hole";

	/// <summary>
	/// Called when we are moving to the next hole.
	/// <para><see cref="int"/>The hole number we are moving to.</para>
	/// </summary>
	public class NextHoleAttribute : EventAttribute
	{
		public NextHoleAttribute() : base( NextHole ) { }
	}

	public const string StateChange = "game.stage.change";

	/// <summary>
	/// Called when the game state has changed.
	/// <para><see cref="GameState"/>The new game state we have changed to.</para>
	/// </summary>
	public class StateChangeAttribute : EventAttribute
	{
		public StateChangeAttribute() : base( StateChange ) { }
	}

	public const string PlayerScored = "player.scored";

	/// <summary>
	/// Called when a player has cupped a ball.
	/// <para><see cref="IClient"/>The client that scored.</para>
	/// <para><see cref="HoleInfo"/>The hole where the client scored.</para>
	/// <para><see cref="int"/>The score the client received.</para>
	/// </summary>
	public class PlayerScoredAttribute : EventAttribute
	{
		public PlayerScoredAttribute() : base( PlayerScored ) { }
	}

	public const string ChatMessageSent = "player.chat.message";

	/// <summary>
	/// Called when a chat message is sent by a player.
	/// <para><see cref="UI.TextChat.ChatMessage"/>A struct containing the details of the chat message.</para>
	/// </summary>
	public class ChatMessageSentAttribute : EventAttribute
	{
		public ChatMessageSentAttribute() : base( ChatMessageSent ) { }
	}
}
