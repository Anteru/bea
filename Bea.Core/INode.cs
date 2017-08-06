namespace Bea.Core
{
	/// <summary>
	/// A node in the build graph
	/// </summary>
	internal interface INode
	{
		void Accept (IGenerator generator);

		string Name { get; }
	}
}
