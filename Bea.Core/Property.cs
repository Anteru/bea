using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bea.Core
{
	public class Property<T>
	{
		public void Set (T value)
		{
			store_ ["_default"] = value;
		}

		public void Set (T value, string config)
		{
			store_ [config] = value;
		}

		public T Get ()
		{
			if (store_.TryGetValue ("_default", out T v))
			{
				return v;
			} else
			{
				return default (T);
			}
		}

		public T Get (string config)
		{
			if (store_.TryGetValue (config, out T v))
			{
				return v;
			} else
			{
				return default (T);
			}
		}

		private Dictionary<string, T> store_ = new Dictionary<string, T> ();
	}
}
