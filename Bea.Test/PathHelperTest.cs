using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bea.Core;

namespace Bea.Test
{
	[TestClass]
	public class PathHelperTest
	{
		[TestMethod]
		public void TestRelativePathFilename ()
		{
			var p1 = @"C:\foo\bar\baz.fi";
			var p2 = @"C:\foo\bar";

			var relativePath = PathHelper.GetRelativePath (p1, p2);

			Assert.AreEqual (@"baz.fi", relativePath);
		}

		[TestMethod]
		public void TestRelativePathFilenameAboveDirectory ()
		{
			var p1 = @"C:\foo\bar\baz.fi";
			var p2 = @"C:\foo\bar\baz\nested";

			var relativePath = PathHelper.GetRelativePath (p1, p2);

			Assert.AreEqual (@"..\..\baz.fi", relativePath);
		}

		[TestMethod]
		public void TestRelativePathFilenameBelowDirectory ()
		{
			var p1 = @"C:\foo\bar\baz.fi";
			var p2 = @"C:\foo\";

			var relativePath = PathHelper.GetRelativePath (p1, p2);

			Assert.AreEqual (@"bar\baz.fi", relativePath);
		}

		[TestMethod]
		public void TestRelativePathDirectory ()
		{
			var p1 = @"C:\foo\bar\baz";
			var p2 = @"C:\foo\bar";

			var relativePath = PathHelper.GetRelativePath (p1, p2);

			Assert.AreEqual (@"baz", relativePath);
		}

		[TestMethod]
		public void TestRelativePathDirectoryAboveDirectory ()
		{
			var p1 = @"C:\foo\bar\baz";
			var p2 = @"C:\foo\bar\baz\nested";

			var relativePath = PathHelper.GetRelativePath (p1, p2);

			Assert.AreEqual (@"..\..\baz", relativePath);
		}

		[TestMethod]
		public void TestRelativePathDirectoryBelowDirectory ()
		{
			var p1 = @"C:\foo\bar\baz";
			var p2 = @"C:\foo\";

			var relativePath = PathHelper.GetRelativePath (p1, p2);

			Assert.AreEqual (@"bar\baz", relativePath);
		}
	}
}
