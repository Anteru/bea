using System.Collections.Generic;

namespace Bea.Build
{
	public class Target
	{
		public List<string> SourceFiles { get; set; } = new List<string>();

		public List<string> Dependencies { get; set; } = new List<string> ();

		public string Name { get; protected set; }
	}
}
