using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectComparer
{
	public interface IComparer<T>
	{
		Comparer<T> AddRule(Func<T, T, bool> condition, Func<T, T, IDifference> differenceFactory);
		Comparer<T> AddRuleForEach<TItem>(Func<T, IEnumerable<TItem>> itemsSelector, Func<TItem, TItem, bool> matchingPredicate, Func<T, T, TItem, IDifference>? addedFactory = null, Func<T, T, TItem, IDifference>? removedFactory = null, Action<Comparer<TItem>>? configureComparer = null);
		IDifference[] Compare(T source, T target);
	}

	public partial class Comparer<T> : IComparer<T>
	{
		private readonly List<IRule> _rules;

		public Comparer()
		{
			_rules = new List<IRule>();
		}

		public Comparer<T> AddRule(Func<T, T, bool> condition, Func<T, T, IDifference> differenceFactory)
		{
			_rules.Add(new Rule(condition, differenceFactory));
			return this;
		}

		public Comparer<T> AddRuleForEach<TItem>(
				Func<T, IEnumerable<TItem>> itemsSelector,
				Func<TItem, TItem, bool> matchingPredicate,
				Func<T, T, TItem, IDifference>? addedFactory = null,
				Func<T, T, TItem, IDifference>? removedFactory = null,
				Action<Comparer<TItem>>? configureComparer = null
			)
		{
			var itemComparer = configureComparer == null ? null : new Comparer<TItem>();
			_rules.Add(new RuleForEach<TItem>(itemsSelector, matchingPredicate, addedFactory, removedFactory, itemComparer));
			return this;
		}

		public IDifference[] Compare(T source, T target)
		{
			return _rules
				.SelectMany(rule => rule.Compare(source, target))
				.ToArray();
		}
	}
}
