namespace ObjectComparer
{

	public partial class Comparer<T>
	{
		interface IRule
		{
			IDifference[] Compare(T source, T target);
		}
	}
}
