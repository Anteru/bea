namespace Bea.Core
{
	public class Executable : Target
	{
		public Property<string> OutputName { get; set; }
		public Property<string> OutputSuffix { get; } = new Property<string> ("");

		public Executable() : base()
		{
			OutputName = new Property<string> (this.GetType().FullName);
		}
	}
}
