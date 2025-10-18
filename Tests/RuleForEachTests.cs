using ObjectComparer;

namespace Tests;

public class RuleForEachTests
{
	[Fact]
	public void AddRuleForEach_ItemAddedToTarget_ShouldDetectAddition()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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
	public void AddRuleForEach_ItemRemovedFromTarget_ShouldDetectRemoval()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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
	public void AddRuleForEach_WithNestedComparer_ShouldDetectItemChanges()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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
	public void AddRuleForEach_WithAllFactories_ShouldDetectAllChangeTypes()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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
	public void AddRuleForEach_WithoutFactories_ShouldNotReportChanges()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id);

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
				new Address { Id = 2, City = "City2" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Empty(differences);
	}

	[Fact]
	public void AddRuleForEach_SupportsMethodChaining()
	{
		// Arrange & Act
		var comparer = new Comparer<Person, IDifference>()
			.AddRuleForEach(
				itemsSelector: p => p.Addresses,
				matchingPredicate: (s, t) => s.Id == t.Id,
				addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City));

		var source = new Person { Addresses = [] };
		var target = new Person { Addresses = [new Address { Id = 1, City = "City1" }] };

		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Single(differences);
	}

	[Fact]
	public void AddRuleForEach_EmptySourceCollection_ShouldDetectAllAsAdded()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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
	public void AddRuleForEach_EmptyTargetCollection_ShouldDetectAllAsRemoved()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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
	public void AddRuleForEach_BothCollectionsEmpty_ShouldReturnNoDifferences()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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
	public void AddRuleForEach_DuplicateKeysInSource_ShouldThrowInvalidOperation()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => comparer.Compare(source, target));
	}

	[Fact]
	public void AddRuleForEach_DuplicateKeysInTarget_ShouldThrowInvalidOperation()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => comparer.Compare(source, target));
	}

	[Fact]
	public void AddRuleForEach_ItemMatchedButNotChanged_WithNestedComparer_ShouldReturnNoDifferences()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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
	public void AddRuleForEach_MultipleItemsAdded_ShouldDetectAll()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
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
				new Address { Id = 2, City = "City2" },
				new Address { Id = 3, City = "City3" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Equal(2, differences.Length);
		Assert.All(differences, d => Assert.IsType<AddressAddedDifference>(d));
	}

	[Fact]
	public void AddRuleForEach_MultipleItemsRemoved_ShouldDetectAll()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
			removedFactory: (s, t, removed) => new AddressRemovedDifference(removed.Id));

		var source = new Person
		{
			Addresses =
			[
				new Address { Id = 1, City = "City1" },
				new Address { Id = 2, City = "City2" },
				new Address { Id = 3, City = "City3" }
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
		Assert.Equal(2, differences.Length);
		Assert.All(differences, d => Assert.IsType<AddressRemovedDifference>(d));
	}
}
