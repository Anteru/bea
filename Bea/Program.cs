using Bea.Core;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bea
{
	class Program
	{
		static void Main(string[] args)
		{
			var provider = new CSharpCodeProvider();
			var parameters = new CompilerParameters();
			parameters.ReferencedAssemblies.Add("Bea.Core.dll");
			parameters.GenerateInMemory = true;
			
			if (args.Length == 1)
			{
				var result = provider.CompileAssemblyFromFile(parameters, args);

				if (result.Errors.HasErrors)
				{
					foreach (CompilerError error in result.Errors)
					{
						var previousForegroundColor = Console.ForegroundColor;
						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write("ERROR");
						Console.ForegroundColor = previousForegroundColor;
						Console.WriteLine(" ({0}, {2}:{3}): {1}", error.ErrorNumber, error.ErrorText, error.Line, error.Column);
					}

					throw new InvalidOperationException();
				}
				
				var assembly = result.CompiledAssembly;

				List<Target> targets = new List<Target> ();
								
				foreach (Type type in assembly.GetTypes ())
				{
					if (typeof(Target).IsAssignableFrom (type))
					{
						targets.Add (Activator.CreateInstance(type) as Target);
					}
				}

				Generator generator = new Generator ();
				generator.Generate (targets, ".", @"E:\Temp\bea");
			}
		}
	}
}
