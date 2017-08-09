using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Bea.Build;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics;

using System.Linq;

namespace Bea
{
	public class Globals
	{
		public Globals (IList<Target> targets)
		{
			targets_ = targets;
		}

		private IList<Target> targets_;

		public Executable AddExecutable (string name)
		{
			var r = new Executable (name);
			targets_.Add (r);
			return r;
		}

		public Library AddLibrary (string name, LibraryType type)
		{
			var r = new Library (name, type);
			targets_.Add (r);
			return r;
		}
	}

	class Program
	{
		static void Main (string [] args)
		{
			string sourcePath = ".";
			string targetPath = @"E:\Temp\Bea";

			ScriptOptions scriptOptions = ScriptOptions.Default
				.WithReferences (
					typeof (Bea.Build.Generator).Assembly,
					typeof (System.Object).Assembly)
				.WithImports ("Bea.Build", "System")
				.WithEmitDebugInformation (false);
			
			var projectFiles = System.IO.Directory.EnumerateFiles (sourcePath, "*.csx", System.IO.SearchOption.AllDirectories).ToList ();

			Console.WriteLine ("Discovered {0} project(s)", projectFiles.Count);

			IList<Target> targets = new List<Target> ();
			var buildContext = new Globals (targets);

			Console.WriteLine ("Evaluating projects ...");
			Stopwatch sw = new Stopwatch ();
			foreach (var projectFile in projectFiles) {
				sw.Restart ();
				Console.WriteLine ("Evaluating '{0}'", projectFile);
				var script = CSharpScript.Create (System.IO.File.OpenRead (projectFile), scriptOptions, typeof (Globals));

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

				script.RunAsync (buildContext).Wait ();
				Console.WriteLine ("Evaluation time: {0}", sw.Elapsed);
			}

			sw.Restart ();
			Console.WriteLine ("Generating build");
			Generator g = new Generator ();
			g.Generate (targets, sourcePath, targetPath);

			Console.WriteLine ("Generation time: {0}", sw.Elapsed);
		}
	}
}
