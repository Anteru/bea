using System;

namespace Bea.Core
{
	internal class CxxExecutableNode : CxxNode
	{
		public CxxExecutableNode (Executable e)
		{
			SourceFiles = e.SourceFiles;
			Name = e.OutputName.Value;

			OutputName = e.OutputName;
			OutputSuffix = e.OutputSuffix;
		}

		public override string Name { get; }

		public override Property<string> OutputName { get; }
		public override Property<string> OutputSuffix { get; }

		public override void Accept (IGenerator generator)
		{
			generator.Visit (this);
		}
	}
}
