namespace ObjectComparer
{
	/// <summary>
	/// Defines a comparison engine that compares two objects of the same type using configurable rules.
	/// </summary>
	/// <typeparam name="TType">The type of objects to compare.</typeparam>
	/// <typeparam name="TDiff">The base type for difference objects returned by comparison rules.</typeparam>
	public interface IComparer<TType, TDiff>
	{
		/// <summary>
		/// Adds a simple comparison rule for top-level properties.
		/// </summary>
		/// <param name="condition">Function that determines if the source and target differ.</param>
		/// <param name="differenceFactory">Function that creates a difference object when the condition is true.</param>
		/// <returns>The comparer instance for method chaining.</returns>
		Comparer<TType, TDiff> AddRule(Func<TType, TType, bool> condition, Func<TType, TType, TDiff> differenceFactory);

		/// <summary>
		/// Adds a collection comparison rule using a matching predicate. Supports complex matching logic but has O(n²) complexity.
		/// Use the keySelector overload for better performance when matching by unique keys.
		/// </summary>
		/// <typeparam name="TItem">The type of items in the collection.</typeparam>
		/// <param name="itemsSelector">Function that selects the collection to compare from the parent object.</param>
		/// <param name="matchingPredicate">Predicate to match items between source and target collections. Must identify items uniquely or throws InvalidOperationException.</param>
		/// <param name="addedFactory">Optional factory to create a difference when an item exists only in the target collection.</param>
		/// <param name="removedFactory">Optional factory to create a difference when an item exists only in the source collection.</param>
		/// <param name="configureComparer">Optional action to configure nested comparison rules for matched items.</param>
		/// <returns>The comparer instance for method chaining.</returns>
		Comparer<TType, TDiff> AddRuleForEach<TItem>(Func<TType, IEnumerable<TItem>> itemsSelector, Func<TItem, TItem, bool> matchingPredicate, Func<TType, TType, TItem, TDiff>? addedFactory = null, Func<TType, TType, TItem, TDiff>? removedFactory = null, Action<Comparer<TItem, TDiff>>? configureComparer = null);

		/// <summary>
		/// Adds a collection comparison rule using a key selector for O(n) performance. Optimized for matching by unique keys (e.g., Id properties).
		/// For complex matching logic beyond simple keys, use the matchingPredicate overload instead.
		/// </summary>
		/// <typeparam name="TItem">The type of items in the collection.</typeparam>
		/// <typeparam name="TKey">The type of the key used for matching items.</typeparam>
		/// <param name="itemsSelector">Function that selects the collection to compare from the parent object.</param>
		/// <param name="keySelector">Function that extracts a unique key from each item for efficient dictionary-based matching.</param>
		/// <param name="addedFactory">Optional factory to create a difference when an item exists only in the target collection.</param>
		/// <param name="removedFactory">Optional factory to create a difference when an item exists only in the source collection.</param>
		/// <param name="configureComparer">Optional action to configure nested comparison rules for matched items.</param>
		/// <returns>The comparer instance for method chaining.</returns>
		Comparer<TType, TDiff> AddRuleForEach<TItem, TKey>(Func<TType, IEnumerable<TItem>> itemsSelector, Func<TItem, TKey> keySelector, Func<TType, TType, TItem, TDiff>? addedFactory = null, Func<TType, TType, TItem, TDiff>? removedFactory = null, Action<Comparer<TItem, TDiff>>? configureComparer = null) where TKey : notnull;

		/// <summary>
		/// Compares two objects using all configured rules and returns an array of differences.
		/// </summary>
		/// <param name="source">The original object.</param>
		/// <param name="target">The modified object to compare against.</param>
		/// <returns>An array of differences found by all rules. Returns empty array if no differences are found.</returns>
		TDiff[] Compare(TType source, TType target);
	}

	/// <summary>
	/// Compares two objects of the same type using configurable rules to detect differences.
	/// </summary>
	/// <typeparam name="TType">The type of objects to compare.</typeparam>
	/// <typeparam name="TDiff">The base type for difference objects returned by comparison rules.</typeparam>
	public partial class Comparer<TType, TDiff> : IComparer<TType, TDiff>
	{
		private readonly List<IRule> _rules;

		/// <summary>
		/// Initializes a new instance of the Comparer class with no rules configured.
		/// </summary>
		public Comparer()
		{
			_rules = [];
		}

		/// <inheritdoc />
		public Comparer<TType, TDiff> AddRule(Func<TType, TType, bool> condition, Func<TType, TType, TDiff> differenceFactory)
		{
			_rules.Add(new Rule(condition, differenceFactory));
			return this;
		}

		/// <inheritdoc />
		public Comparer<TType, TDiff> AddRuleForEach<TItem>(
			Func<TType, IEnumerable<TItem>> itemsSelector,
			Func<TItem, TItem, bool> matchingPredicate,
			Func<TType, TType, TItem, TDiff>? addedFactory = null,
			Func<TType, TType, TItem, TDiff>? removedFactory = null,
			Action<Comparer<TItem, TDiff>>? configureComparer = null)
		{
			Comparer<TItem, TDiff>? itemComparer = null;
			if (configureComparer is not null)
			{
				itemComparer = new Comparer<TItem, TDiff>();
				configureComparer(itemComparer);
			}

			_rules.Add(new RuleForEach<TItem>(itemsSelector, matchingPredicate, addedFactory, removedFactory, itemComparer));

			return this;
		}

		/// <inheritdoc />
		public Comparer<TType, TDiff> AddRuleForEach<TItem, TKey>(
			Func<TType, IEnumerable<TItem>> itemsSelector,
			Func<TItem, TKey> keySelector,
			Func<TType, TType, TItem, TDiff>? addedFactory = null,
			Func<TType, TType, TItem, TDiff>? removedFactory = null,
			Action<Comparer<TItem, TDiff>>? configureComparer = null) where TKey : notnull
		{
			Comparer<TItem, TDiff>? itemComparer = null;
			if (configureComparer is not null)
			{
				itemComparer = new Comparer<TItem, TDiff>();
				configureComparer(itemComparer);
			}

			_rules.Add(new RuleForEachWithKey<TItem, TKey>(itemsSelector, keySelector, addedFactory, removedFactory, itemComparer));

			return this;
		}

		/// <inheritdoc />
		public TDiff[] Compare(TType source, TType target)
		{
			return [.. _rules.SelectMany(rule => rule.Compare(source, target))];
		}
	}
}
