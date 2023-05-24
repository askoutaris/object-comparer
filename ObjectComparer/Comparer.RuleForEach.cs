using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectComparer
{

	public partial class Comparer<TType, TDiff>
	{
		class RuleForEach<TItem> : IRule
		{
			public Func<TType, IEnumerable<TItem>> ItemsSelector { get; }
			public Func<TItem, TItem, bool> MatchingPredicate { get; }
			public Func<TType, TType, TItem, TDiff>? AddedFactory { get; }
			public Func<TType, TType, TItem, TDiff>? RemovedFactory { get; }
			public Comparer<TItem, TDiff>? ItemComparer { get; }

			public RuleForEach(
				Func<TType, IEnumerable<TItem>> itemsSelector,
				Func<TItem, TItem, bool> matchingPredicate,
				Func<TType, TType, TItem, TDiff>? addedFactory,
				Func<TType, TType, TItem, TDiff>? removedFactory,
				Comparer<TItem, TDiff>? itemComparer)
			{
				ItemsSelector = itemsSelector;
				MatchingPredicate = matchingPredicate;
				AddedFactory = addedFactory;
				RemovedFactory = removedFactory;
				ItemComparer = itemComparer;
			}

			public TDiff[] Compare(TType source, TType target)
			{
				var sourceItems = ItemsSelector(source);
				var targetItems = ItemsSelector(target);

				var differences = new List<TDiff>();
				foreach (var sourceItem in sourceItems)
				{
					TItem? matchedItem = targetItems.SingleOrDefault(targetItem => MatchingPredicate(sourceItem, targetItem));

					if (matchedItem != null && ItemComparer != null)
						differences.AddRange(ItemComparer.Compare(sourceItem, matchedItem));
					else if (matchedItem == null && RemovedFactory != null)
						differences.Add(RemovedFactory(source, target, sourceItem));
				}

				foreach (var targetItem in targetItems)
				{
					TItem? matchedItem = sourceItems.SingleOrDefault(sourceItem => MatchingPredicate(targetItem, sourceItem));

					if (matchedItem == null && AddedFactory != null)
						differences.Add(AddedFactory(source, target, targetItem));
				}

				return differences.ToArray();
			}
		}
	}
}
