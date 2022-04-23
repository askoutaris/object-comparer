using System;

namespace ObjectComparer
{

	public partial class Comparer<T>
	{
		class Rule : IRule
		{
			public Func<T, T, bool> IsDifferent { get; }
			public Func<T, T, IDifference> DifferenceFactory { get; }

			public Rule(Func<T, T, bool> isDifferent, Func<T, T, IDifference> differenceFactory)
			{
				IsDifferent = isDifferent;
				DifferenceFactory = differenceFactory;
			}

			public IDifference[] Compare(T source, T target)
			{
				if (IsDifferent(source, target))
					return new[] { DifferenceFactory(source, target) };
				else
					return Array.Empty<IDifference>();
			}
		}
	}
}
