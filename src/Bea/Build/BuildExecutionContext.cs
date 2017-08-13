using System.Collections.Generic;

namespace Bea.Build
{
	public class BuildExecutionContext
	{
		public BuildExecutionContext (IList<Target> targets)
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
