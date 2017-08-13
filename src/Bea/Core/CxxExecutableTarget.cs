using Bea.Build;

namespace Bea.Core
{
	internal class CxxExecutableTarget : CxxTarget
	{
		public CxxExecutableTarget (Executable e) : base (e)
		{
		}

		public override void Accept (IGenerator generator)
		{
			generator.Visit (this);
		}
	}
}
