namespace ObjectComparer
{
	public partial class Comparer<TType, TDiff>
	{
		interface IRule
		{
			TDiff[] Compare(TType source, TType target);
		}
	}
}
