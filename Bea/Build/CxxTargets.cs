namespace Bea.Build
{
	public class CxxBinary : Target
	{
		public Property<string> OutputName { get; set; }
		public Property<string> OutputSuffix { get; } = new Property<string> ("");
		public PreprocessorDefinitions PreprocessorDefinitions { get; } = new PreprocessorDefinitions ();

		public CxxBinary (string name)
		{
			Name = name;
			OutputName = new Property<string> (Name);
		}
	}

	public class Executable : CxxBinary
	{
		public Executable (string name) : base (name)
		{
		}
	}

	public enum LibraryType
	{
		Static,
		Shared
	}

	public class Library : CxxBinary
	{
		public LibraryType Type { get; }
		public Library (string name, LibraryType type) : base (name)
		{
			Type = type;
		}
	}
}
