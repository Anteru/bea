using Bea.Core;

public class HelloBea : Executable
{
	public HelloBea()
	{
		SourceFiles.AddRange(new string[] { "HelloBeaMain.cpp" });

		OutputSuffix.Set ("Debug", "d");
	}
}
