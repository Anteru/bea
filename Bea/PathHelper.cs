using System;

namespace Bea.Core
{
	public class PathHelper
	{
		public static string GetRelativePath (string source, string target)
		{
			if (!target.EndsWith ("\\")) {
				target += '\\';
			}
			Uri path1 = new Uri (source);
			Uri path2 = new Uri (target);
			Uri diff = path2.MakeRelativeUri (path1);

			return diff.ToString ().Replace ('/', '\\');
		}
	}
}
