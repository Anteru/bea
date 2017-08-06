namespace Bea.Core
{
	public class Executable : Target
	{
		public Property<string> ExecutableName { get; set; } = new Property<string> ();

		public Executable() : base()
		{
			ExecutableName.Set (this.GetType().FullName);
		}
	}
}
