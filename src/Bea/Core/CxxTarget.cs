using Bea.Build;
using System.Collections.Generic;

namespace Bea.Core
{
	internal abstract class CxxTarget : ITarget
	{
		public abstract void Accept (IGenerator generator);

		public List<string> IncludeDirectories { get; set; } = new List<string>();
		public List<string> SourceFiles { get; set;  } = new List<string> ();

		public List<ITarget> Dependencies { get; set; } = new List<ITarget> ();
		public string Name { get; }

		public string OutputName { get; }
		public string OutputSuffix { get; }

		public PreprocessorDefinitions PreprocessorDefinitions { get; }

		public CxxTarget (CxxBinary target)
		{
			Name = target.Name;

			OutputName = target.OutputName;
			OutputSuffix = target.OutputSuffix;

			SourceFiles = target.SourceFiles;
			SourceFiles.Sort ();

			foreach (var dependency in target.Dependencies) {
				switch (dependency.Value) {
					case string s:
						unlinkedDependencies_.Add (s);
						break;
					case Target t:
						unlinkedDependencies_.Add (t.Name);
						break;
				}
			}

			PreprocessorDefinitions = target.PreprocessorDefinitions;
		}

		public void LinkDependencies (IDictionary<string, ITarget> nodes)
		{
			foreach (var dependency in unlinkedDependencies_) {
				Dependencies.Add (nodes [dependency]);
			}

			unlinkedDependencies_ = null;
		}

		private IList<string> unlinkedDependencies_ = new List<string>();
	}
}
