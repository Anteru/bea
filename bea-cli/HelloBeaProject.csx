var helloBeaLib = AddLibrary ("HelloBeaLib", LibraryType.Static);
helloBeaLib.SourceFiles.AddRange (new string [] {"HelloBeaLib.cpp"});

var helloBea = AddExecutable ("HelloBea");
helloBea.SourceFiles.AddRange (new string [] {"HelloBeaMain.cpp"});
helloBea.PreprocessorDefinitions.Add ("BUILD_BEA", 1);
helloBea.OutputSuffix.Set ("Debug", "d");
helloBea.Dependencies.Add (helloBeaLib);