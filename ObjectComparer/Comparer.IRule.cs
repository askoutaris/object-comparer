namespace ObjectComparer
{
	public partial class Comparer<TType, TDiff>
	{
		/// <summary>
		/// Internal interface for comparison rules. All rules must implement this to be executed by the comparer.
		/// </summary>
		interface IRule
		{
			/// <summary>
			/// Compares the source and target objects according to this rule's logic.
			/// </summary>
			/// <param name="source">The original object.</param>
			/// <param name="target">The modified object to compare against.</param>
			/// <returns>An array of differences found by this rule. Returns empty array if no differences are found.</returns>
			TDiff[] Compare(TType source, TType target);
		}
	}
}
