using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bea.Core
{
	public class Target
	{
		public List<string> SourceFiles { get; set; } = new List<string>();

		public List<string> Dependencies { get; set; } = new List<string> ();
	}
}
