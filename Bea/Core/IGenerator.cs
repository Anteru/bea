namespace Bea.Core
{
	internal interface IGenerator
	{
		void Visit (CxxExecutableTarget node);
		void Visit (CxxLibraryTarget node);
	}
}
