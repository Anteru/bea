using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bea.Core
{
	/// <summary>
	/// A build property. These can be configuration-specific (for instance,
	/// a project may choose to have a different name for debug variants).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Property<T>
	{
		public T Value
		{
			get;
		}

		public Property (T value)
		{
			Value = value;
		}

		public void Set (string config, T value)
		{
			if (config == null) {
				throw new ArgumentNullException (nameof (config));
			}

			if (variants_ == null) {
				variants_ = new Dictionary<string, T> ();
			}

			variants_ [config] = value;
		}

		public T Get (string config)
		{
			if (variants_?.ContainsKey (config) ?? false) {
				return variants_ [config];
			} else {
				return Value;
			}
		}

		public bool HasConfiguration (string config)
		{
			return variants_?.ContainsKey (config) ?? false;
		}

		public ICollection<string> Configurations => variants_.Keys;

		private Dictionary<string, T> variants_;
	}
}
