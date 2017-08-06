using System.Collections.Generic;

namespace Bea.Core
{
	internal abstract class CxxNode : INode
	{
		public abstract void Accept (IGenerator generator);

		public List<string> IncludeDirectories { get; set; } = new List<string>();
		public List<string> SourceFiles { get; set;  } = new List<string> ();

		public List<INode> Dependencies { get; set; } = new List<INode> ();
		public string Name { get; }

		public Property<string> OutputName { get; }
		public Property<string> OutputSuffix { get; }

		public CxxNode (CxxBinary target)
		{
			Name = target.OutputName.Value;

			OutputName = target.OutputName;
			OutputSuffix = target.OutputSuffix;

			SourceFiles = target.SourceFiles;
		}
	}
}
