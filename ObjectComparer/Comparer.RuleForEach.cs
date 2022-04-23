using System;
using System.Collections.Generic;

namespace ObjectComparer
{

	public partial class Comparer<T>
	{
		class RuleForEach<TItem> : IRule
		{
			public Func<T, IEnumerable<TItem>> ItemsSelector { get; }
			public Func<TItem, TItem, bool> MatchingPredicate { get; }
			public Func<T, T, TItem, IDifference>? AddedFactory { get; }
			public Func<T, T, TItem, IDifference>? RemovedFactory { get; }
			public Comparer<TItem>? ItemComparer { get; }

			public RuleForEach(
				Func<T, IEnumerable<TItem>> itemsSelector,
				Func<TItem, TItem, bool> matchingPredicate,
				Func<T, T, TItem, IDifference>? addedFactory,
				Func<T, T, TItem, IDifference>? removedFactory,
				Comparer<TItem>? itemComparer)
			{
				ItemsSelector = itemsSelector;
				MatchingPredicate = matchingPredicate;
				AddedFactory = addedFactory;
				RemovedFactory = removedFactory;
				ItemComparer = itemComparer;
			}

			public IDifference[] Compare(T source, T target)
			{
				var sourceItems = ItemsSelector(source);
				var targetItems = ItemsSelector(target);

				var differences = new List<IDifference>();
				foreach (var sourceItem in sourceItems)
				{
					TItem? matchedItem = default;
					foreach (var targetItem in targetItems)
					{
						if (MatchingPredicate(sourceItem, targetItem))
						{
							matchedItem = targetItem;
							break;
						}
					}

					if (matchedItem != null && ItemComparer != null)
						differences.AddRange(ItemComparer.Compare(sourceItem, matchedItem));
					else if (matchedItem == null && AddedFactory != null)
						differences.Add(AddedFactory(source, target, sourceItem));
				}

				foreach (var targetItem in targetItems)
				{
					TItem? matchedItem = default;
					foreach (var sourceItem in sourceItems)
					{
						if (MatchingPredicate(targetItem, sourceItem))
						{
							matchedItem = sourceItem;
							break;
						}
					}

					if (matchedItem == null && RemovedFactory != null)
						differences.Add(RemovedFactory(source, target, targetItem));
				}

				return differences.ToArray();
			}
		}
	}
}
