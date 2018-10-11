using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Bea.Core
{
	namespace VisualStudio2017
	{
		class ConfigurationPlatform
		{
			public ConfigurationPlatform (string configuration, string platform)
			{
				Configuration = configuration;
				Platform = platform;
			}

			public string Configuration { get;  }
			public string Platform { get; }

			public override string ToString ()
			{
				return $"{Configuration}|{Platform}";
			}
		}

		class ConfigurationPlatforms
		{
			public IEnumerable<string> Configurations { get; private set; }
			public IEnumerable<string> Platforms { get; private set; }

			public IEnumerable<ConfigurationPlatform> Enumerate ()
			{
				foreach (var config in Configurations) {
					foreach (var platform in Platforms) {
						yield return new ConfigurationPlatform (config, platform);
					}
				}

				yield break;
			}

			public ConfigurationPlatforms ()
			{
				Configurations = new List<string> { "Debug", "Release" };
				Platforms = new List<string> { "x64" };
			}
		}

		enum ProjectType
		{
			Executable,
			StaticLibrary,
			SharedLibrary
		}

		abstract class Project
		{
			protected ConfigurationPlatforms configurationPlatforms_;

			protected readonly Guid guid_ = Guid.NewGuid ();
			
			public abstract string Name { get; }
			public abstract string Path { get; }
			public abstract CxxTarget Target { get; }
			public Guid Guid => guid_;
			public ProjectType Type { get; }

			public abstract void Write (IDictionary<ITarget, Project> targetToProject);

			public Project (ConfigurationPlatforms configurationPlatforms,
				ProjectType type)
			{
				configurationPlatforms_ = configurationPlatforms;
				Type = type;
			}

			protected XElement GetProjectConfigurations ()
			{
				var root = new XElement ("ItemGroup",
					new XAttribute ("Label", "ProjectConfigurations"));

				foreach (var configPlatform in configurationPlatforms_.Enumerate ()) {
					var element = new XElement ("ProjectConfiguration",
						new XAttribute ("Include", $"{configPlatform}"),
						new XElement ("Configuration", configPlatform.Configuration),
						new XElement ("Platform", configPlatform.Platform));

					root.Add (element);
				}

				return root;
			}

			protected XElement GetProjectGlobals ()
			{
				return new XElement ("PropertyGroup",
					new XAttribute ("Label", "Globals"),
					new XElement ("VCProjectVersion", "15.0"),
					new XElement ("ProjectGuid", $"{{{Guid.ToString ().ToUpper ()}}}"),
					new XElement ("Keyword", "Win32Proj"),
					// TODO: Does this need to be written at all?
					new XElement ("RootNamespace", Name),
					new XElement ("WindowsTargetPlatformVersion", "10.0.15063.0"));
			}

			protected IEnumerable<XElement> GetProjectConfigurationProperties ()
			{
				List<XElement> result = new List<XElement> ();

				foreach (var configPlatform in configurationPlatforms_.Enumerate ()) {
					var element = new XElement ("PropertyGroup",
							new XAttribute ("Condition", $"'$(Configuration)|$(Platform)'=='{configPlatform}'"),
							new XAttribute ("Label", "Configuration"),
							new XElement ("PlatformToolset", "v141"),
							new XElement ("CharacterSet", "Unicode"));
					
					switch (Type) {
						case ProjectType.Executable: {
								element.Add (new XElement ("ConfigurationType", "Application"));
								break;
							}

						case ProjectType.StaticLibrary: {
								element.Add (new XElement ("ConfigurationType", "StaticLibrary"));
								break;
							}
					}

					if (IsDebugConfiguration (configPlatform.Configuration)) {
						element.Add (new XElement ("UseDebugLibraries", "true"));
					} else {
						element.Add (new XElement ("UseDebugLibraries", "false"));
					}

					result.Add (element);
				}

				return result;
			}

			protected IEnumerable<XElement> GetProjectImports ()
			{
				List<XElement> result = new List<XElement>
				{
					new XElement ("Import",
						new XAttribute ("Project", @"$(VCTargetsPath)\Microsoft.Cpp.Default.props")),
					new XElement ("Import",
						new XAttribute ("Project", @"$(VCTargetsPath)\Microsoft.Cpp.props")),
					new XElement ("Import",
						new XAttribute ("Project", @"$(VCTargetsPath)\Microsoft.Cpp.targets")),

					new XElement ("ImportGroup",
						new XAttribute ("Label", "ExtensionSettings")),
					new XElement ("ImportGroup",
						new XAttribute ("Label", "Shared"))
				};

				// TODO: Write this per platform only?
				foreach (var configPlatform in configurationPlatforms_.Enumerate ()) {
					var element = new XElement ("ImportGroup",
							new XAttribute ("Condition", $"'$(Configuration)|$(Platform)'=='{configPlatform}'"),
							new XAttribute ("Label", "PropertySheets"),
							new XElement ("Import",
								new XAttribute ("Project", @"$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"),
								new XAttribute ("Condition", @"exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"),
								new XAttribute ("Label", "LocalAppDataPlatform"))
							);

					result.Add (element);
				}

				return result;
			}

			protected IEnumerable<XElement> GetCompileOptions ()
			{
				List<XElement> result = new List<XElement> ();

				foreach (var configPlatform in configurationPlatforms_.Enumerate ()) {
					var clOptions = new XElement ("ClCompile",
								new XElement ("PrecompiledHeader"),
								new XElement ("WarningLevel", "Level3"));

					var preprocessorDefinitions = new List<string> ();

					foreach (var def in Target.PreprocessorDefinitions.Definitions) {
						if (def.Value != null) {
							preprocessorDefinitions.Add ($"{def.Key}={def.Value}");
						} else {
							preprocessorDefinitions.Add ($"{def.Key}");
						}
					}

					if (configPlatform.Platform == "x86") {
						preprocessorDefinitions.Add ("WIN32");
					}

					if (IsDebugConfiguration (configPlatform.Configuration)) {
						clOptions.Add (new XElement ("Optimization", "Disabled"));
						preprocessorDefinitions.Add ("_DEBUG");
					} else {
						clOptions.Add (new XElement ("Optimization", "MaxSpeed"));
						clOptions.Add (new XElement ("FunctionLevelLinking", true));
						clOptions.Add (new XElement ("IntrinsicFunctions", true));
						preprocessorDefinitions.Add ("NDEBUG");
					}

					var defGroup = new XElement ("ItemDefinitionGroup",
						new XAttribute ("Condition", $"'$(Configuration)|$(Platform)'=='{configPlatform}'"));

					if (Type == ProjectType.StaticLibrary) {
						preprocessorDefinitions.Add ("_LIB");

						var linkOptions = new XElement ("Link",
							new XElement ("SubSystem", "Windows"));

						if (IsDebugConfiguration (configPlatform.Configuration)) {
							//
						} else {
							linkOptions.Add (
								new XElement ("EnableCOMDATFolding", true),
								new XElement ("OptimizeReferences", true));
						}

						defGroup.Add (linkOptions);
					}

					preprocessorDefinitions.Add ("%(PreprocessorDefinitions)");

					clOptions.Add (new XElement ("PreprocessorDefinitions",
						String.Join<string> (";", preprocessorDefinitions))); 

					defGroup.Add (clOptions);

					result.Add (defGroup);
				}

				return result;
			}

			protected static bool IsDebugConfiguration (string config)
			{
				return config.Contains ("Debug");
			}
		}

		static class XmlExtensions
		{
			public static void AddRange (this XElement e, IEnumerable<XElement> items)
			{
				foreach (var item in items) {
					e.Add (item);
				}
			}
		}

		class CxxProject : Project
		{
			protected IEnumerable<XElement> GetLinkSettings ()
			{
				var expressionParser = new ExpressionParser ();

				List<XElement> result = new List<XElement> ();

				foreach (var configPlatform in configurationPlatforms_.Enumerate ()) {
					var element = new XElement ("PropertyGroup",
							new XAttribute ("Condition", $"'$(Configuration)|$(Platform)'=='{configPlatform}'"),
							new XElement ("LinkIncremental", false));

					ExpressionEvaluationContext ctx = new ExpressionEvaluationContext
					{
						Configuration = configPlatform.Configuration,
						Platform = configPlatform.Platform
					};
					
					var name = expressionParser.Parse (Target.OutputName).Evaluate (ctx);
					var suffix = expressionParser.Parse (Target.OutputSuffix).Evaluate (ctx);

					element.Add (new XElement ("TargetName",
						$"{name}{suffix}"));
					
					result.Add (element);
				}

				return result;
			}

			protected XElement GetProjectReferences (IDictionary<ITarget, Project> nodeToProjectMapping)
			{
				var group = new XElement ("ItemGroup");

				foreach (var dependency in Target.Dependencies) {
					var project = nodeToProjectMapping [dependency];

					var projectReference = new XElement ("ProjectReference",
						new XAttribute ("Include", PathHelper.GetRelativePath (project.Path, System.IO.Path.GetDirectoryName (Path))),
						new XElement ("Project",
							$"{{{project.Guid}}}"));

					group.Add (projectReference);
				}

				return group;
			}

			private CxxProject (string sourcePath, string targetPath, CxxTarget target, 
				ConfigurationPlatforms configurationPlatforms, ProjectType type)
				: base (configurationPlatforms, type)
			{
				targetPath_ = System.IO.Path.Combine (targetPath, target.Name + ".vcxproj");
				sourcePath_ = sourcePath;
				Target = target;
			}

			public CxxProject (string sourcePath, string targetPath,
				CxxExecutableTarget target, ConfigurationPlatforms configurationPlatforms)
				: this (sourcePath, targetPath, target as CxxTarget, configurationPlatforms, ProjectType.Executable)
			{
			}

			public CxxProject (string sourcePath, string targetPath,
				CxxLibraryTarget target, ConfigurationPlatforms configurationPlatforms) 
				: this (sourcePath, targetPath, target as CxxTarget, configurationPlatforms, ProjectType.StaticLibrary)
			{
			}

			public override CxxTarget Target { get; }
			public override string Path => targetPath_;

			public override string Name => Target.Name;

			public override void Write (IDictionary<ITarget, Project> nodeToProjectMap)
			{
				var project = new XElement ("Project",
						new XAttribute ("DefaultTargets", "Build"),
						new XAttribute ("ToolsVersion", "15.0"));

				project.AddRange (GetProjectConfigurationProperties ());
				project.Add (GetProjectConfigurations ());
				project.Add (GetProjectGlobals ());
				project.AddRange (GetLinkSettings ());
				project.AddRange (GetProjectImports ());
				project.AddRange (GetCompileOptions ());
				
				if (Target.Dependencies.Count > 0) {
					project.Add (GetProjectReferences (nodeToProjectMap));
				}

				var sourceFileGroup = new XElement ("ItemGroup");
				foreach (var sourceFile in Target.SourceFiles) {
					var ext = System.IO.Path.GetExtension (sourceFile).ToLower ();
					var fullPath = System.IO.Path.Combine (sourcePath_, sourceFile);

					switch (ext) {
						case ".c":
						case ".cxx":
						case ".cpp":
						sourceFileGroup.Add (new XElement ("ClCompile",
							new XAttribute ("Include", fullPath)));
						break;

						case ".h":
						case ".hpp":
						case ".hxx":
							sourceFileGroup.Add (new XElement ("ClInclude",
								new XAttribute ("Include", fullPath)));
						break;

						default:
							break;
					}
			}

				project.Add (sourceFileGroup);

				XDocument root = new XDocument (
					new XDeclaration ("1.0", "utf-8", "yes"),
					project
					);

				XNamespace xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
				foreach (var node in root.Descendants ()) {
					node.Name = xmlns + node.Name.LocalName;
				}

				root.Save (targetPath_);
			}

			private readonly string targetPath_;
			private readonly string sourcePath_;
		}

		class Generator : IGenerator
		{
			private readonly string targetPath_;
			private readonly string sourcePath_;

			private ConfigurationPlatforms configurationPlatforms_ = new ConfigurationPlatforms ();

			private Dictionary<ITarget, Project> projects_ = new Dictionary<ITarget, Project>();
	
			public Generator (string sourcePath, string targetPath)
			{
				sourcePath_ = sourcePath;
				targetPath_ = targetPath;
			}

			public void Generate ()
			{
				foreach (var project in projects_.Values) {
					project.Write (projects_);
				}

				using (TextWriter tw = File.CreateText (Path.Combine (targetPath_, "test.sln"))) {
					tw.WriteLine ("Microsoft Visual Studio Solution File, Format Version 12.00");
					tw.WriteLine ("# Visual Studio 15");
					tw.WriteLine ("VisualStudioVersion = 15.0.26430.16");
					tw.WriteLine ("MinimumVisualStudioVersion = 10.0.40219.1");

					foreach (var project in projects_.Values) {
						// C++ project GUID: 8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942
						var relativeProjectPath = PathHelper.GetRelativePath (project.Path, targetPath_);
						var projectGuid = project.Guid.ToString ().ToUpper ();
						tw.WriteLine ($"Project(\"{{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}}\") = \"{project.Name}\", \"{relativeProjectPath}\", \"{{{projectGuid}}}\"");
						tw.WriteLine ("EndProject");
					}

					tw.WriteLine ("Global");
					tw.WriteLine ("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
					foreach (var configPlatform in configurationPlatforms_.Enumerate ()) {
						tw.WriteLine ($"\t\t{configPlatform} = {configPlatform}");
					}
					tw.WriteLine ("\tEndGlobalSection");

					tw.WriteLine ("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
					foreach (var project in projects_.Values) {
						foreach (var configPlatform in configurationPlatforms_.Enumerate ()) {
							var projectGuid = project.Guid.ToString ().ToUpper ();
							tw.WriteLine ($"\t\t{{{projectGuid}}}" +
								$".{configPlatform}.ActiveCfg " +
								$"= {configPlatform}");
							tw.WriteLine ($"\t\t{{{projectGuid}}}" +
								$".{configPlatform}.Build.0 " +
								$"= {configPlatform}");
						}
					}
					tw.WriteLine ("\tEndGlobalSection");

					tw.WriteLine ("\tGlobalSection(SolutionProperties) = postSolution");
					tw.WriteLine ("\t\tHideSolutionNode = FALSE");
					tw.WriteLine ("\tEndGlobalSection");

					tw.WriteLine ("EndGlobal");
				}
			}

			public void Visit (CxxExecutableTarget target)
			{
				projects_ [target] = new CxxProject (
					sourcePath_, targetPath_, target, configurationPlatforms_);
			}

			public void Visit (CxxLibraryTarget target)
			{
				projects_ [target] = new CxxProject (
					sourcePath_, targetPath_, target, configurationPlatforms_);
			}
		}
	}
}
