using ObjectComparer;

namespace Tests;

public class RuleForEachWithKeyTests
{
	[Fact]
	public void AddRuleForEachWithKey_ItemAddedToTarget_ShouldDetectAddition()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City));

		var source = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" }
			]
		};

		var target = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" },
				new Address { Id = 2, City = "City2" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Single(differences);
		var diff = Assert.IsType<AddressAddedDifference>(differences[0]);
		Assert.Equal(2, diff.AddressId);
		Assert.Equal("City2", diff.City);
	}

	[Fact]
	public void AddRuleForEachWithKey_ItemRemovedFromTarget_ShouldDetectRemoval()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			removedFactory: (s, t, removed) => new AddressRemovedDifference(removed.Id));

		var source = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" },
				new Address { Id = 2, City = "City2" }
			]
		};

		var target = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Single(differences);
		var diff = Assert.IsType<AddressRemovedDifference>(differences[0]);
		Assert.Equal(2, diff.AddressId);
	}

	[Fact]
	public void AddRuleForEachWithKey_WithNestedComparer_ShouldDetectItemChanges()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			configureComparer: itemComparer => itemComparer
				.AddRule(
					condition: (s, t) => s.City != t.City,
					differenceFactory: (s, t) => new AddressCityChangedDifference(s.Id, s.City, t.City)));

		var source = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" }
			]
		};

		var target = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1-Updated" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Single(differences);
		var diff = Assert.IsType<AddressCityChangedDifference>(differences[0]);
		Assert.Equal(1, diff.AddressId);
		Assert.Equal("City1", diff.OldCity);
		Assert.Equal("City1-Updated", diff.NewCity);
	}

	[Fact]
	public void AddRuleForEachWithKey_WithAllFactories_ShouldDetectAllChangeTypes()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City),
			removedFactory: (s, t, removed) => new AddressRemovedDifference(removed.Id),
			configureComparer: itemComparer => itemComparer
				.AddRule(
					condition: (s, t) => s.City != t.City,
					differenceFactory: (s, t) => new AddressCityChangedDifference(s.Id, s.City, t.City)));

		var source = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" },
				new Address { Id = 2, City = "City2" }
			]
		};

		var target = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1-Updated" },
				new Address { Id = 3, City = "City3" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Equal(3, differences.Length);
		Assert.Contains(differences, d => d is AddressCityChangedDifference);
		Assert.Contains(differences, d => d is AddressRemovedDifference);
		Assert.Contains(differences, d => d is AddressAddedDifference);
	}

	[Fact]
	public void AddRuleForEachWithKey_EmptySourceCollection_ShouldDetectAllAsAdded()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City));

		var source = new Person { Addresses = [] };
		var target = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" },
				new Address { Id = 2, City = "City2" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Equal(2, differences.Length);
		Assert.All(differences, d => Assert.IsType<AddressAddedDifference>(d));
	}

	[Fact]
	public void AddRuleForEachWithKey_EmptyTargetCollection_ShouldDetectAllAsRemoved()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			removedFactory: (s, t, removed) => new AddressRemovedDifference(removed.Id));

		var source = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" },
				new Address { Id = 2, City = "City2" }
			]
		};
		var target = new Person { Addresses = [] };

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Equal(2, differences.Length);
		Assert.All(differences, d => Assert.IsType<AddressRemovedDifference>(d));
	}

	[Fact]
	public void AddRuleForEachWithKey_BothCollectionsEmpty_ShouldReturnNoDifferences()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City),
			removedFactory: (s, t, removed) => new AddressRemovedDifference(removed.Id));

		var source = new Person { Addresses = [] };
		var target = new Person { Addresses = [] };

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Empty(differences);
	}

	[Fact]
	public void AddRuleForEachWithKey_DuplicateKeysInSource_ShouldThrowArgumentException()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City));

		var source = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" },
				new Address { Id = 1, City = "City1-Duplicate" }
			]
		};
		var target = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" }
			]
		};

		// Act & Assert - Dictionary throws ArgumentException for duplicate keys
		Assert.Throws<ArgumentException>(() => comparer.Compare(source, target));
	}

	[Fact]
	public void AddRuleForEachWithKey_DuplicateKeysInTarget_ShouldThrowArgumentException()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			removedFactory: (s, t, removed) => new AddressRemovedDifference(removed.Id));

		var source = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" }
			]
		};
		var target = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" },
				new Address { Id = 1, City = "City1-Duplicate" }
			]
		};

		// Act & Assert - Dictionary throws ArgumentException for duplicate keys
		Assert.Throws<ArgumentException>(() => comparer.Compare(source, target));
	}

	[Fact]
	public void AddRuleForEachWithKey_ItemMatchedButNotChanged_WithNestedComparer_ShouldReturnNoDifferences()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			configureComparer: itemComparer => itemComparer
				.AddRule(
					condition: (s, t) => s.City != t.City,
					differenceFactory: (s, t) => new AddressCityChangedDifference(s.Id, s.City, t.City)));

		var source = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" }
			]
		};
		var target = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Empty(differences);
	}

	[Fact]
	public void AddRuleForEachWithKey_LargeCollection_ShouldPerformEfficiently()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			keySelector: a => a.Id,
			addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City),
			removedFactory: (s, t, removed) => new AddressRemovedDifference(removed.Id));

		// Create large collections (1000 items)
		var sourceAddresses = Enumerable.Range(1, 1000)
			.Select(i => new Address { Id = i, City = $"City{i}" })
			.ToArray();

		var targetAddresses = Enumerable.Range(500, 1000)
			.Select(i => new Address { Id = i, City = $"City{i}" })
			.ToArray();

		var source = new Person { Addresses = sourceAddresses };
		var target = new Person { Addresses = targetAddresses };

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		// 499 removed (1-499) + 499 added (1001-1499) = 998 differences
		// Common items: 500-1000 (501 items with no changes)
		Assert.Equal(998, differences.Length);
		Assert.Equal(499, differences.Count(d => d is AddressRemovedDifference));
		Assert.Equal(499, differences.Count(d => d is AddressAddedDifference));
	}

	[Fact]
	public void AddRuleForEachWithKey_SupportsMethodChaining()
	{
		// Arrange & Act
		var comparer = new Comparer<Person, IDifference>()
			.AddRuleForEach(
				itemsSelector: p => p.Addresses,
				keySelector: a => a.Id,
				addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City));

		var source = new Person { Addresses = [] };
		var target = new Person { Addresses = [new Address { Id = 1, City = "City1" }] };

		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Single(differences);
	}
}
