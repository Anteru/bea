using System.Collections.Generic;

namespace Bea.Core
{
	internal abstract class CxxNode : INode
	{
		public abstract void Accept (IGenerator generator);

		public List<string> IncludeDirectories { get; set; } = new List<string>();
		public List<string> SourceFiles { get; set;  } = new List<string> ();

		public List<INode> Dependencies { get; set; } = new List<INode> ();
		public abstract string Name { get; }

		public abstract Property<string> OutputName { get; }
		public abstract Property<string> OutputSuffix { get; }
	}
}
