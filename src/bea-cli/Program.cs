using System;
using System.Collections.Generic;
using Bea.Build;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics;

using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Bea
{

	class Program
	{
		static void EvaluateProjects (IEnumerable<string> projectFiles, 
			BuildExecutionContext buildExecContext)
		{
			Stopwatch sw = new Stopwatch ();
			sw.Start ();

			foreach (var projectFile in projectFiles) {
				Console.WriteLine ("Evaluating '{0}'", projectFile);

				ScriptOptions scriptOptions = ScriptOptions.Default
					.WithReferences (
						typeof (Bea.Build.Generator).Assembly,
						typeof (System.Object).Assembly)
					.WithImports ("Bea.Build", "System")
					.WithEmitDebugInformation (false);

				var script = CSharpScript.Create (System.IO.File.OpenRead (projectFile),
					scriptOptions, typeof (BuildExecutionContext));

				var compilationResult = script.Compile ();

				foreach (var diagnostic in compilationResult) {
					var oldConsoleColor = Console.ForegroundColor;
					switch (diagnostic.Severity) {
						case DiagnosticSeverity.Error: Console.ForegroundColor = ConsoleColor.Red; break;
						case DiagnosticSeverity.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
					}
					Console.WriteLine (diagnostic);
					Console.ForegroundColor = oldConsoleColor;
				}

				script.RunAsync (buildExecContext).Wait ();
				Console.WriteLine ("Evaluation time: {0}", sw.Elapsed);
			}
		}

		private static void Generate (string sourcePath, string targetPath)
		{
			var projectFiles = System.IO.Directory.EnumerateFiles (sourcePath, "*.csx", System.IO.SearchOption.AllDirectories).ToList ();
			projectFiles.Sort ();

			Console.WriteLine ("Discovered {0} project(s)", projectFiles.Count);

			var buildEnv = new BuildEnvironment (sourcePath, targetPath);
			var buildExecContext = buildEnv.ExecutionContext;

			Console.WriteLine ("Evaluating projects ...");
			Stopwatch sw = new Stopwatch ();

			EvaluateProjects (projectFiles, buildExecContext);

			sw.Restart ();
			Console.WriteLine ("Generating build");

			buildEnv.Generate ();

			Console.WriteLine ("Generation time: {0}", sw.Elapsed);
		}

		static void Main (string [] args)
		{
			CommandLineApplication app = new CommandLineApplication
			{
				Name = "bea-cli",
				Description = "Bea command line runner"
			};

			app.HelpOption ("-?|-h|--help");

			app.Command ("generate", (c) =>
			{
				var source = c.Argument ("source", "Source directory");
				var target = c.Argument ("target", "Target directory");

				c.OnExecute (() =>
				{
					Generate (source.Value, target.Value);
					return 0;
				});
			});

			app.Execute (args);
		}
	}
}
