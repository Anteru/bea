using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		abstract class Project
		{
			protected ConfigurationPlatforms configurationPlatforms_;

			protected readonly Guid guid_ = Guid.NewGuid ();
			
			public abstract string Name { get; }
			public abstract string Path { get; }
			public Guid Guid => guid_;

			public abstract void Write ();

			public Project (ConfigurationPlatforms configurationPlatforms)
			{
				configurationPlatforms_ = configurationPlatforms;
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
							new XElement ("ConfigurationType", "Application"),
							new XElement ("PlatformToolset", "v141"),
							new XElement ("CharacterSet", "Unicode"));

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
					var options = new XElement ("ClCompile",
								new XElement ("PrecompiledHeader"),
								new XElement ("WarningLevel", "Level3"));

					if (IsDebugConfiguration (configPlatform.Configuration)) {
						options.Add (new XElement ("Optimization", "Disabled"));
						options.Add (new XElement ("PreprocessorDefinitions",
							"WIN32;_DEBUG;_CONSOLE;%(PreprocessorDefinitions)"));
					} else {
						options.Add (new XElement ("Optimization", "MaxSpeed"));
						options.Add (new XElement ("FunctionLevelLinking", true));
						options.Add (new XElement ("IntrinsicFunctions", true));
						options.Add (new XElement ("PreprocessorDefinitions",
							"WIN32;NDEBUG;_CONSOLE;%(PreprocessorDefinitions)"));
					}

					result.Add (new XElement ("ItemDefinitionGroup",
						new XAttribute ("Condition", $"'$(Configuration)|$(Platform)'=='{configPlatform}'"),
						options));
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
				List<XElement> result = new List<XElement> ();

				foreach (var configPlatform in configurationPlatforms_.Enumerate ()) {
					var element = new XElement ("PropertyGroup",
							new XAttribute ("Condition", $"'$(Configuration)|$(Platform)'=='{configPlatform}'"),
							new XElement ("LinkIncremental", false));

					if (Node.OutputName.HasConfiguration (configPlatform.Configuration) || Node.OutputSuffix.HasConfiguration (configPlatform.Configuration)) {
						var name = Node.OutputName.Get (configPlatform.Configuration);
						var suffix = Node.OutputSuffix.Get (configPlatform.Configuration);

						element.Add (new XElement ("TargetName",
							$"{name}{suffix}"));
					}

					result.Add (element);
				}

				return result;
			}

			private CxxProject (string sourcePath, string targetPath, CxxNode node, ConfigurationPlatforms configurationPlatforms) : base (configurationPlatforms)
			{
				targetPath_ = System.IO.Path.Combine (targetPath, node.Name + ".vcxproj");
				sourcePath_ = sourcePath;
				Node = node;

			}

			public CxxProject (string sourcePath, string targetPath,
				CxxExecutableNode node, ConfigurationPlatforms configurationPlatforms) : this (sourcePath, targetPath, node as CxxNode, configurationPlatforms)
			{
			}

			public CxxProject (string sourcePath, string targetPath,
				CxxLibraryNode node, ConfigurationPlatforms configurationPlatforms) : this (sourcePath, targetPath, node as CxxNode, configurationPlatforms)
			{
			}

			public CxxNode Node { get; }
			public override string Path => targetPath_;

			public override string Name => Node.Name;

			public override void Write ()
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

				var sourceFileGroup = new XElement ("ItemGroup");
				foreach (var sourceFile in Node.SourceFiles) {
					sourceFileGroup.Add (new XElement ("ClCompile",
						new XAttribute ("Include", System.IO.Path.Combine (sourcePath_, sourceFile))));
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

			private string targetPath_;
			private string sourcePath_;
		}

		class Generator : IGenerator
		{
			private string targetPath_;
			private string sourcePath_;

			private ConfigurationPlatforms configurationPlatforms_ = new ConfigurationPlatforms ();

			private Dictionary<INode, Project> projects_ = new Dictionary<INode, Project>();
	
			public Generator (string sourcePath, string targetPath)
			{
				sourcePath_ = sourcePath;
				targetPath_ = targetPath;
			}

			public void Generate ()
			{
				foreach (var project in projects_.Values) {
					project.Write ();
				}

				using (TextWriter tw = File.CreateText (Path.Combine (targetPath_, "test.sln"))) {
					tw.WriteLine ("Microsoft Visual Studio Solution File, Format Version 12.00");
					tw.WriteLine ("# Visual Studio 15");
					tw.WriteLine ("VisualStudioVersion = 15.0.26430.16");
					tw.WriteLine ("MinimumVisualStudioVersion = 10.0.40219.1");

					foreach (var project in projects_.Values) {
						// C++ project GUID: 8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942
						var relativeProjectPath = project.Path.Substring (targetPath_.Length).TrimStart (new char [] { '\\' });
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

			public void Visit (CxxExecutableNode node)
			{
				projects_ [node] = new CxxProject (
					sourcePath_, targetPath_, node, configurationPlatforms_);
			}

			public void Visit (CxxLibraryNode node)
			{
				projects_ [node] = new CxxProject (
					sourcePath_, targetPath_, node, configurationPlatforms_);
			}
		}
	}
}
