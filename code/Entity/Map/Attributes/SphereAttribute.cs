using Sandbox;
using System;
using System.Text;

namespace Hammer
{
	/// <summary>
	/// Display a sphere of the specified property's radius in Hammer.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class )]
	public class SphereAttribute : MetaDataAttribute
	{
		internal string Property;

		public SphereAttribute( string property ) => Property = property;

		public override void AddHeader( StringBuilder sb )
		{
			sb.Append( $" sphere( {StringX.QuoteSafe(Property)} )" );
		}
	}
}
