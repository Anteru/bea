using System.Collections.Generic;

namespace Bea.Core
{
	/// <summary>
	/// A node in the build graph
	/// </summary>
	internal interface INode
	{
		void Accept (IGenerator generator);

		string Name { get; }
		List<INode> Dependencies { get; }

		void LinkDependencies (IDictionary<string, INode> nodes);
	}
}
