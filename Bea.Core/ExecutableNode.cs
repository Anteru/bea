namespace Bea.Core
{
	internal class ExecutableNode : INode
	{
		public Executable Executable { get; private set; }

		public ExecutableNode (Executable e)
		{
			Executable = e;
		}

		public void Accept (IGenerator generator)
		{
			generator.Visit (this);
		}
	}
}
