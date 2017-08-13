using Bea.Core;
using System.Collections.Generic;
using System.IO;

namespace Bea.Build
{
	public class Generator
	{
		public void Generate (IEnumerable<Target> targets, string sourcePath, string targetPath)
		{
			sourcePath = Path.GetFullPath (sourcePath);
			targetPath = Path.GetFullPath (targetPath);

			Dictionary<string, ITarget> nodes = new Dictionary<string, ITarget> ();

			foreach (var target in targets) {
				switch (target) {
					case Executable e: {
							nodes [e.Name] = new CxxExecutableTarget (e);
							break;
						}

					case Library l: {
							nodes [l.Name] = new CxxLibraryTarget (l);
							break;
						}
				}
			}

			foreach (var node in nodes.Values) {
				node.LinkDependencies (nodes);
			}

			var vsGen = new Bea.Core.VisualStudio2017.Generator (sourcePath, targetPath);
			foreach (var node in nodes) {
				node.Value.Accept (vsGen);
			}

			vsGen.Generate ();
		}
	}
}
