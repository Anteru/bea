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
		void Visit (ExecutableNode node);
	}

	public class Generator
	{
		public void Generate (IEnumerable<Target> targets, string sourcePath, string targetPath)
		{
			sourcePath = Path.GetFullPath (sourcePath);
			targetPath = Path.GetFullPath (targetPath);

			List<INode> nodes = new List<INode> ();

			foreach (var target in targets) {
				switch (target) {
					case Executable e: {
							nodes.Add (new ExecutableNode (e));
							break;
						}
				}
			}

			var vsGen = new VisualStudio2017.Generator (sourcePath, targetPath);
			foreach (var node in nodes) {
				node.Accept (vsGen);
			}

			vsGen.Generate ();
		}
	}
}
