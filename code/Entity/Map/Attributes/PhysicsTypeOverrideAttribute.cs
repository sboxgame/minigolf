using Sandbox;
using System;
using System.Text;

namespace Hammer
{
	/// <summary>
	/// Use this to indicate to the map builder that any meshes associated with this entity should have a different default physics type.
	/// </summary>
	/// <remarks>
	/// Adds the base class PhysicsTypeOverride_* that the map builder is hard coded to use.
	/// </remarks>
	[AttributeUsage( AttributeTargets.Class )]
	public class PhysicsTypeOverrideAttribute : MetaDataAttribute
	{
		public enum PhysicsTypeOverride
		{
			Mesh,
			SingleConvex,
			MultiConvex
		};

		internal PhysicsTypeOverride PhysTypeOverride;

		public PhysicsTypeOverrideAttribute( PhysicsTypeOverride type ) => PhysTypeOverride = type;

		public override void AddHeader( StringBuilder sb )
		{
			// Seems a bit saner then relying on Enum.ToString() in the string builder..
			var name = PhysTypeOverride switch {
				PhysicsTypeOverride.Mesh => "Mesh",
				PhysicsTypeOverride.SingleConvex => "SingleConvex",
				PhysicsTypeOverride.MultiConvex => "MultiConvex",
				_ => throw new NotSupportedException()
			};

			// todo: multiple bases
			sb.Append( $" base( PhysicsTypeOverride_{PhysTypeOverride} ) " );
		}
	}
}
