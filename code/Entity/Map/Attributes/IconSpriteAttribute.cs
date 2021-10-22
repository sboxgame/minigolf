using System;
using System.Text;

namespace Hammer
{
	[AttributeUsage( AttributeTargets.Class )]
	public class IconSpriteAttribute : MetaDataAttribute
	{
		internal string Icon;

		public IconSpriteAttribute( string icon )
		{
			Icon = icon;
		}

		public override void AddHeader( StringBuilder sb )
		{
			sb.Append( $" iconsprite( \"{Icon}\" )" );
		}
	}
}
