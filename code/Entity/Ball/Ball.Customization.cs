﻿using Facepunch.Customization;
using Sandbox;

namespace Facepunch.Minigolf.Entities;

public partial class Ball
{

	private int parthash = -1;
	private Particles trail;

	[Net]
	public AnimatedEntity Hat { get; set; }
	private AnimatedEntity localhat;

	private void CleanupCustomization()
	{
		if ( Sandbox.Game.IsServer )
		{
			if ( Hat.IsValid() ) Hat.Delete();
			if ( trail != null ) trail.Destroy();
		}

		if ( Sandbox.Game.IsClient )
		{
			if ( localhat.IsValid() ) localhat.Delete();
		}

		Hat = null;
		trail = null;
		localhat = null;
	}

	[GameEvent.Tick.Server]
	private void EnsureCustomization()
	{
		var cc = Client.Components.GetOrCreate<CustomizeComponent>();

		var hash = cc.GetPartsHash();
		if ( hash == parthash ) return;

		parthash = hash;
		ApplyCustomization();
	}

	private void ApplyCustomization()
	{
		var cc = Client.Components.GetOrCreate<CustomizeComponent>();

		// todo: could use an ez mode for setting skins and models
		// especially for client/local pawn stuff like hat..

		CleanupCustomization();
		CleanupCustomizationOnClient();

		var trailpart = cc.GetEquippedPart( "Trails" );
		if ( trailpart != null )
		{
			trail = Particles.Create( trailpart.AssetPath, this );
			trail.SetPosition( 1, Vector3.One ); // Color
		}

		var hatpart = cc.GetEquippedPart( "Hats" );
		if ( hatpart != null )
		{
			Hat = new AnimatedEntity( hatpart.AssetPath );
			Hat.Owner = Client.Pawn as Entity;
		}

		var skinpart = cc.GetEquippedPart( "Skins" );
		if ( skinpart != null )
		{
			SetSkinOnClient( To.Everyone, NetworkIdent, skinpart.AssetPath );
		}
	}

	[ClientRpc]
	private void CleanupCustomizationOnClient()
	{
		CleanupCustomization();
	}

	[ConCmd.Client( "setskin", CanBeCalledFromServer = true )]
	public static void SetSkinOnClient( int ballIdent, string assetPath )
	{
		var ball = Entity.FindByIndex( ballIdent );
		if ( ball is not Ball b ) return;

		b.SetMaterialOverride( assetPath );
	}

}
