namespace ObjectComparer
{
	public partial class Comparer<TType, TDiff>
	{
		/// <summary>
		/// Simple comparison rule that evaluates a condition and produces a difference when the condition is true.
		/// </summary>
		class Rule : IRule
		{
			/// <summary>
			/// Function that determines if the source and target objects differ.
			/// </summary>
			public Func<TType, TType, bool> IsDifferent { get; }

			/// <summary>
			/// Function that creates a difference object when the condition is true.
			/// </summary>
			public Func<TType, TType, TDiff> DifferenceFactory { get; }

			/// <summary>
			/// Initializes a new instance of the Rule class.
			/// </summary>
			/// <param name="isDifferent">Function that determines if the source and target differ.</param>
			/// <param name="differenceFactory">Function that creates a difference object when the condition is true.</param>
			public Rule(Func<TType, TType, bool> isDifferent, Func<TType, TType, TDiff> differenceFactory)
			{
				IsDifferent = isDifferent;
				DifferenceFactory = differenceFactory;
			}

			/// <inheritdoc />
			public TDiff[] Compare(TType source, TType target)
			{
				if (IsDifferent(source, target))
					return [DifferenceFactory(source, target)];
				else
					return [];
			}
		}
	}
}
