using System.Collections.Generic;

namespace Bea.Build
{
	public class TargetReference
	{
		public object Value { get; private set; }

		public static implicit operator TargetReference (string name)
		{
			return new TargetReference { Value = name };
		}

		public static implicit operator TargetReference (Target t)
		{
			return new TargetReference { Value = t };
		}
	}

	public class Target
	{
		public List<string> SourceFiles { get; set; } = new List<string>();

		public List<TargetReference> Dependencies { get; set; } = new List<TargetReference> ();

		public string Name { get; protected set; }
	}
}
