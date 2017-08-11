using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bea.Build
{
	public sealed class BuildEnvironment
	{
		public BuildEnvironment (IList<Target> targets)
		{
			targets_ = targets;
		}

		private IList<Target> targets_;

		public Executable AddExecutable (string name)
		{
			var r = new Executable (name);
			targets_.Add (r);
			return r;
		}

		public Library AddLibrary (string name, LibraryType type)
		{
			var r = new Library (name, type);
			targets_.Add (r);
			return r;
		}
	}
}
