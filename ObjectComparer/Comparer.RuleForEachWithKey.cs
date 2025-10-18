namespace ObjectComparer
{
	public partial class Comparer<TType, TDiff>
	{
		/// <summary>
		/// Collection comparison rule that uses a key selector for O(n) performance with dictionary-based lookups.
		/// Optimized for matching items by unique keys. For complex matching logic, use RuleForEach instead.
		/// </summary>
		/// <typeparam name="TItem">The type of items in the collection.</typeparam>
		/// <typeparam name="TKey">The type of the key used for matching items. Must be non-null.</typeparam>
		class RuleForEachWithKey<TItem, TKey> : IRule where TKey : notnull
		{
			/// <summary>
			/// Function that selects the collection to compare from the parent object.
			/// </summary>
			public Func<TType, IEnumerable<TItem>> ItemsSelector { get; }

			/// <summary>
			/// Function that extracts a unique key from each item for efficient dictionary-based matching.
			/// Keys must be unique within each collection or throws ArgumentException.
			/// </summary>
			public Func<TItem, TKey> KeySelector { get; }

			/// <summary>
			/// Optional factory to create a difference when an item exists only in the target collection.
			/// </summary>
			public Func<TType, TType, TItem, TDiff>? AddedFactory { get; }

			/// <summary>
			/// Optional factory to create a difference when an item exists only in the source collection.
			/// </summary>
			public Func<TType, TType, TItem, TDiff>? RemovedFactory { get; }

			/// <summary>
			/// Optional nested comparer to detect differences in matched items.
			/// </summary>
			public Comparer<TItem, TDiff>? ItemComparer { get; }

			/// <summary>
			/// Initializes a new instance of the RuleForEachWithKey class.
			/// </summary>
			/// <param name="itemsSelector">Function that selects the collection to compare.</param>
			/// <param name="keySelector">Function that extracts a unique key from each item.</param>
			/// <param name="addedFactory">Optional factory for added items.</param>
			/// <param name="removedFactory">Optional factory for removed items.</param>
			/// <param name="itemComparer">Optional nested comparer for matched items.</param>
			public RuleForEachWithKey(
				Func<TType, IEnumerable<TItem>> itemsSelector,
				Func<TItem, TKey> keySelector,
				Func<TType, TType, TItem, TDiff>? addedFactory,
				Func<TType, TType, TItem, TDiff>? removedFactory,
				Comparer<TItem, TDiff>? itemComparer)
			{
				ItemsSelector = itemsSelector;
				KeySelector = keySelector;
				AddedFactory = addedFactory;
				RemovedFactory = removedFactory;
				ItemComparer = itemComparer;
			}

			/// <inheritdoc />
			public TDiff[] Compare(TType source, TType target)
			{
				var sourceItems = ItemsSelector(source);
				var targetItems = ItemsSelector(target);

				// Build dictionaries for O(1) lookups
				var sourceDict = sourceItems.ToDictionary(item => KeySelector(item));
				var targetDict = targetItems.ToDictionary(item => KeySelector(item));

				var differences = new List<TDiff>();

				// Check for removed items and changed items
				foreach (var kvp in sourceDict)
				{
					var key = kvp.Key;
					var sourceItem = kvp.Value;

					if (targetDict.TryGetValue(key, out var targetItem))
					{
						// Item exists in both - check for changes
						if (ItemComparer != null)
							differences.AddRange(ItemComparer.Compare(sourceItem, targetItem));
					}
					else
					{
						// Item was removed
						if (RemovedFactory != null)
							differences.Add(RemovedFactory(source, target, sourceItem));
					}
				}

				// Check for added items
				foreach (var kvp in targetDict)
				{
					var key = kvp.Key;
					var targetItem = kvp.Value;

					if (!sourceDict.ContainsKey(key))
					{
						if (AddedFactory != null)
							differences.Add(AddedFactory(source, target, targetItem));
					}
				}

				return [.. differences];
			}
		}
	}
}
