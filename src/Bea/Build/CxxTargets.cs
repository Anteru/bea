namespace Bea.Build
{
	public class CxxBinary : Target
	{
		public string OutputName { get; set; }
		public string OutputSuffix { get; set; } = string.Empty;
		public PreprocessorDefinitions PreprocessorDefinitions { get; } = new PreprocessorDefinitions ();

		public CxxBinary (string name)
		{
			Name = name;
			OutputName = Name;
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
