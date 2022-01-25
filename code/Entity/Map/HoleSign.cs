using Sandbox;

using Facepunch.Minigolf.UI;

namespace Facepunch.Minigolf.Entities;

[Library( "minigolf_hole_sign", Description = "Minigolf Sign Pole" )]
[Hammer.DrawAngles]
[Hammer.EditorSprite( "editor/snd_event.vmat" )]
public partial class HoleSign : Entity
{
	HoleWorldPanel WorldPanel;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		WorldPanel = new();
		WorldPanel.Transform = Transform;

		// Bring it out the smallest amount from the sign
		WorldPanel.Transform = WorldPanel.Transform.WithPosition( WorldPanel.Transform.Position + WorldPanel.Transform.Rotation.Forward * 0.05f );
	}
}
