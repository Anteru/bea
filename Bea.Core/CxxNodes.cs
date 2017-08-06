using System;

namespace Bea.Core
{
	internal class CxxExecutableNode : CxxNode
	{
		public CxxExecutableNode (Executable e) : base (e)
		{
		}

		public override void Accept (IGenerator generator)
		{
			generator.Visit (this);
		}
	}

	internal class CxxLibraryNode : CxxNode
	{
		public CxxLibraryNode (Library l) : base (l)
		{
		}

		public override void Accept (IGenerator generator)
		{
			generator.Visit (this);
		}
	}
}
