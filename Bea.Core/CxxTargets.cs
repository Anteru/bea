﻿namespace Bea.Core
{
	public class CxxBinary : Target
	{
		public Property<string> OutputName { get; set; }
		public Property<string> OutputSuffix { get; } = new Property<string> ("");

		public CxxBinary ()
		{
			Name = this.GetType ().FullName;
			OutputName = new Property<string> (Name);
		}
	}

	public class Executable : CxxBinary
	{
	}

	public enum LibraryType
	{
		Static,
		Shared
	}

	public class Library : CxxBinary
	{
		public LibraryType Type { get; }
		public Library (LibraryType type)
		{
			Type = type;
		}
	}
}
