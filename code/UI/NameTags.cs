using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Minigolf
{
	public class BaseNameTag : Panel
	{
		public Label NameLabel;
		public Image Avatar;

		Entity entity;

		public BaseNameTag( Entity entity )
		{
			this.entity = entity;

			var client = entity.Client;

			NameLabel = Add.Label( $"{client.Name}" );
			Avatar = Add.Image( $"avatar:{client.SteamId}" );
		}

		public virtual void UpdateFromPlayer( Entity entity )
		{
			// Nothing to do unless we're showing health and shit
		}
	}

	public class NameTags : Panel
	{
		Dictionary<Entity, BaseNameTag> ActiveTags = new Dictionary<Entity, BaseNameTag>();

		public NameTags()
		{
			StyleSheet.Load("/ui/NameTags.scss");
		}

		public override void Tick()
		{
			base.Tick();

			var deleteList = new List<Entity>();
			deleteList.AddRange(ActiveTags.Keys);

			foreach (var entity in Entity.All.OfType<Ball>().OrderBy(x => Vector3.DistanceBetween(x.EyePos, CurrentView.Position ) ))
			{
				if (UpdateNameTag(entity))
					deleteList.Remove(entity);
			}

			foreach (var player in deleteList)
			{
				ActiveTags[player].Delete();
				ActiveTags.Remove(player);
			}
		}

		public virtual BaseNameTag CreateNameTag( Entity entity )
		{
			var tag = new BaseNameTag(entity);
			tag.Parent = this;
			return tag;
		}

		public bool UpdateNameTag( Entity entity )
		{
			// Don't draw local player
			// if ( entity == Local.Pawn )
			// 	return false;

			// If there's a ball without an owner remove it
			if ( !entity.Client.IsValid() )
				return false;

			var labelPos = entity.Position + Vector3.Up * 16;

			// Are we looking in this direction?
			// var lookDir = (labelPos - CurrentView.Position).Normal;
			// if (CurrentView.Rotation.Forward.Dot(lookDir) < 0.5)
			// 	return false;

			float dist = labelPos.Distance( CurrentView.Position );
			var objectSize = 0.05f / dist / (2.0f * MathF.Tan((CurrentView.FieldOfView / 2.0f).DegreeToRadian())) * 3000.0f;

			objectSize = objectSize.Clamp(0.25f, 0.5f);

			if (!ActiveTags.TryGetValue(entity, out var tag))
			{
				tag = CreateNameTag(entity);
				ActiveTags[entity] = tag;
			}

			tag.UpdateFromPlayer(entity);

			var screenPos = labelPos.ToScreen();

			tag.Style.Left = Length.Fraction(screenPos.x);
			tag.Style.Top = Length.Fraction(screenPos.y);
			tag.Style.Opacity = 1;

			var transform = new PanelTransform();
			transform.AddTranslateY(Length.Fraction(-1.0f));
			transform.AddScale(objectSize);
			transform.AddTranslateX(Length.Fraction(-0.5f));

			tag.Style.Transform = transform;
			tag.Style.Dirty();

			return true;
		}
	}
}
