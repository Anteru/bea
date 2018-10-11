﻿using System.Collections.Generic;

namespace Bea.Build
{
	public class BuildEnvironment
	{
		public BuildEnvironment (string sourcePath, string targetPath)
		{
			ExecutionContext = new BuildExecutionContext (targets_);
			sourcePath_ = sourcePath;
			targetPath_ = targetPath;
		}

		private readonly IList<Target> targets_ = new List<Target> ();
		private Generator generator_ = new Generator ();
		private readonly string sourcePath_;
		private readonly string targetPath_;

		public BuildExecutionContext ExecutionContext { get; }

		public void Generate ()
		{
			generator_.Generate (targets_, sourcePath_, targetPath_);
		}
	}
}
