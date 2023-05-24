using System;

namespace ObjectComparer
{

	public partial class Comparer<TType, TDiff>
	{
		class Rule : IRule
		{
			public Func<TType, TType, bool> IsDifferent { get; }
			public Func<TType, TType, TDiff> DifferenceFactory { get; }

			public Rule(Func<TType, TType, bool> isDifferent, Func<TType, TType, TDiff> differenceFactory)
			{
				IsDifferent = isDifferent;
				DifferenceFactory = differenceFactory;
			}

			public TDiff[] Compare(TType source, TType target)
			{
				if (IsDifferent(source, target))
					return new[] { DifferenceFactory(source, target) };
				else
					return Array.Empty<TDiff>();
			}
		}
	}
}
