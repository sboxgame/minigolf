using System.Collections.Generic;

public struct SteamID
{
	ulong ID { get; set; }

	public SteamID( ulong id )
	{
		ID = id;
	}

	public uint AccountID
	{
		get => (uint)this[0, 0xFFFFFFFF];
		set => this[0, 0xFFFFFFFF] = value;
	}

	public uint AccountInstance
	{
		get => (uint)this[32, 0xFFFFF];
		set => this[32, 0xFFFFF] = value;
	}

	public SteamIDAccountType AccountType
	{
		get => (SteamIDAccountType)this[52, 0xF];
		set => this[52, 0xF] = (ulong)value;
	}

	public SteamIDUniverse AccountUniverse
	{
		get => (SteamIDUniverse)this[56, 0xFF];
		set => this[56, 0xFF] = (ulong)value;
	}


	/// <summary>
	/// A static account key used for grouping accounts with differing instances.
	/// </summary>
	public ulong StaticAccountKey => ((ulong)AccountUniverse << 56) + ((ulong)AccountType << 52) + AccountID;

	public override string ToString()
	{
		if ( !AccountTypeChars.TryGetValue( AccountType, out var accountTypeChar ) )
		{
			accountTypeChar = UnknownAccountTypeChar;
		}

		bool renderInstance = false;

		switch ( AccountType )
		{
			case SteamIDAccountType.AnonGameServer:
			case SteamIDAccountType.Multiseat:
				renderInstance = true;
				break;

			case SteamIDAccountType.Individual:
				renderInstance = (AccountInstance != DesktopInstance);
				break;
		}

		if ( renderInstance )
		{
			return $"[{accountTypeChar}:{(uint)AccountUniverse}:{AccountID}:{AccountInstance}]";
		}

		return $"[{accountTypeChar}:{(uint)AccountUniverse}:{AccountID}]";
	}

	private ulong this[uint bitoffset, ulong valuemask]
	{
		get => (ID >> (ushort)bitoffset) & valuemask;
		set => ID = (ID & ~(valuemask << (ushort)bitoffset)) | ((value & valuemask) << (ushort)bitoffset);
	}

	static readonly Dictionary<SteamIDAccountType, char> AccountTypeChars = new()
	{
		{ SteamIDAccountType.AnonGameServer, 'A' },
		{ SteamIDAccountType.GameServer, 'G' },
		{ SteamIDAccountType.Multiseat, 'M' },
		{ SteamIDAccountType.Pending, 'P' },
		{ SteamIDAccountType.ContentServer, 'C' },
		{ SteamIDAccountType.Clan, 'g' },
		{ SteamIDAccountType.Chat, 'T' },
        { SteamIDAccountType.Invalid, 'I' },
		{ SteamIDAccountType.Individual, 'U' },
		{ SteamIDAccountType.AnonUser, 'a' },
	};

	const char UnknownAccountTypeChar = 'i';

	/// <summary>
	/// The account instance value when representing all instanced <see cref="SteamID">SteamIDs</see>.
	/// </summary>
	public const uint AllInstances = 0;
	/// <summary>
	/// The account instance value for a desktop <see cref="SteamID"/>.
	/// </summary>
	public const uint DesktopInstance = 1;
	/// <summary>
	/// The account instance value for a console <see cref="SteamID"/>.
	/// </summary>
	public const uint ConsoleInstance = 2;
	/// <summary>
	/// The account instance for mobile or web based <see cref="SteamID">SteamIDs</see>.
	/// </summary>
	public const uint WebInstance = 4;
}

public enum SteamIDAccountType
{
	Invalid = 0,
	Individual = 1,
	Multiseat = 2,
	GameServer = 3,
	AnonGameServer = 4,
	Pending = 5,
	ContentServer = 6,
	Clan = 7,
	Chat = 8,
	ConsoleUser = 9,
	AnonUser = 10,
}

public enum SteamIDUniverse
{
	Invalid = 0,
	Public = 1,
	Beta = 2,
	Internal = 3,
	Dev = 4,
}
