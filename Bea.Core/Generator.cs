using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bea.Core
{
	internal interface IGenerator
	{
		void Visit (CxxExecutableNode node);
		void Visit (CxxLibraryNode node);
	}

	public class Generator
	{
		public void Generate (IEnumerable<Target> targets, string sourcePath, string targetPath)
		{
			sourcePath = Path.GetFullPath (sourcePath);
			targetPath = Path.GetFullPath (targetPath);

			Dictionary<string, INode> nodes = new Dictionary<string, INode> ();

			foreach (var target in targets) {
				switch (target) {
					case Executable e: {
							nodes [e.Name] = new CxxExecutableNode (e);
							break;
						}

					case Library l: {
							nodes [l.Name] = new CxxLibraryNode (l);
							break;
						}
				}
			}

			foreach (var node in nodes.Values) {
				node.LinkDependencies (nodes);
			}

			var vsGen = new VisualStudio2017.Generator (sourcePath, targetPath);
			foreach (var node in nodes) {
				node.Value.Accept (vsGen);
			}

			vsGen.Generate ();
		}
	}
}
