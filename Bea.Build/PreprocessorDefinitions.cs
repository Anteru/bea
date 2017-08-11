using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bea.Build
{
	public sealed class PreprocessorDefinitions
	{
		public void Add (string name)
		{
			definitions_ [name] = null;
		}

		public void Add (string name, object value)
		{
			if (value == null) {
				throw new ArgumentNullException (nameof (value));
			}

			definitions_ [name] = value.ToString ();
		}

		public void Remove (string name)
		{
			definitions_.Remove (name);
		}

		public IReadOnlyDictionary<string, string> Definitions => definitions_;

		private Dictionary<string, string> definitions_ = new Dictionary<string, string> ();
	}
}
