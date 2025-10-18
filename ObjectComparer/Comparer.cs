namespace ObjectComparer
{
	public interface IComparer<TType, TDiff>
	{
		Comparer<TType, TDiff> AddRule(Func<TType, TType, bool> condition, Func<TType, TType, TDiff> differenceFactory);
		Comparer<TType, TDiff> AddRuleForEach<TItem>(Func<TType, IEnumerable<TItem>> itemsSelector, Func<TItem, TItem, bool> matchingPredicate, Func<TType, TType, TItem, TDiff>? addedFactory = null, Func<TType, TType, TItem, TDiff>? removedFactory = null, Action<Comparer<TItem, TDiff>>? configureComparer = null);
		TDiff[] Compare(TType source, TType target);
	}

	public partial class Comparer<TType, TDiff> : IComparer<TType, TDiff>
	{
		private readonly List<IRule> _rules;

		public Comparer()
		{
			_rules = new List<IRule>();
		}

		public Comparer<TType, TDiff> AddRule(Func<TType, TType, bool> condition, Func<TType, TType, TDiff> differenceFactory)
		{
			_rules.Add(new Rule(condition, differenceFactory));
			return this;
		}

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

		public TDiff[] Compare(TType source, TType target)
		{
			return _rules
				.SelectMany(rule => rule.Compare(source, target))
				.ToArray();
		}
	}
}
