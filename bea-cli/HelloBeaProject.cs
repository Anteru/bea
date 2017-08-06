using Bea.Build;

public class HelloBea : Executable
{
	public HelloBea()
	{
		SourceFiles.AddRange(new string[] { "HelloBeaMain.cpp" });

		PreprocessorDefinitions.Add ("BUILD_BEA", 1);
		OutputSuffix.Set ("Debug", "d");
		Dependencies.Add ("HelloBeaLib");
	}
}

public class HelloBeaLib : Library
{
	public HelloBeaLib () : base (LibraryType.Static)
	{
		SourceFiles.AddRange (new string [] { "HelloBeaLib.cpp" });
	}
}
