using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bea.Build;

namespace Bea.Core
{
	internal class CxxLibraryTarget : CxxTarget
	{
		public CxxLibraryTarget (Library l) : base (l)
		{
		}

		public override void Accept (IGenerator generator)
		{
			generator.Visit (this);
		}
	}
}
