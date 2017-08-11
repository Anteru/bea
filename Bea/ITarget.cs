using System.Collections.Generic;

namespace Bea.Core
{

	/// <summary>
	/// A node in the build graph
	/// </summary>
	internal interface ITarget
	{
		void Accept (IGenerator generator);

		string Name { get; }
		List<ITarget> Dependencies { get; }

		void LinkDependencies (IDictionary<string, ITarget> targets);
	}
}
