using System;

namespace Minigolf
{
	static class ColorConvert
	{

		public static Color HSLToRGB( int h, float s, float l )
		{
			float r = 0;
			float g = 0;
			float b = 0;

			if ( s == 0 )
			{
				r = g = b = l;
			}
			else
			{
				float v1, v2;
				float hue = (float)h / 360;

				v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
				v1 = 2 * l - v2;

				r = HueToRGB( v1, v2, hue + (1.0f / 3) );
				g = HueToRGB( v1, v2, hue );
				b = HueToRGB( v1, v2, hue - (1.0f / 3) );
			}

			return new Color( r, g, b );
		}

		private static float HueToRGB( float v1, float v2, float vH )
		{
			if ( vH < 0 )
				vH += 1;

			if ( vH > 1 )
				vH -= 1;

			if ( (6 * vH) < 1 )
				return (v1 + (v2 - v1) * 6 * vH);

			if ( (2 * vH) < 1 )
				return v2;

			if ( (3 * vH) < 2 )
				return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

			return v1;
		}
	}
}
