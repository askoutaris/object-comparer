using ObjectComparer;

namespace Tests;

public class ComparerTests
{
	[Fact]
	public void Compare_NoRules_ShouldReturnEmptyArray()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		var source = new Person { Name = "John", Age = 30 };
		var target = new Person { Name = "Jane", Age = 25 };

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Empty(differences);
	}

	[Fact]
	public void Compare_MixingSimpleAndCollectionRules_ShouldDetectAllDifferences()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRule(
			condition: (s, t) => s.Name != t.Name,
			differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));
		comparer.AddRule(
			condition: (s, t) => s.Age != t.Age,
			differenceFactory: (s, t) => new AgeChangedDifference(s.Age, t.Age));
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
			Name = "John",
			Age = 30,
			Addresses =
			[
				new Address { Id = 1, City = "City1" },
				new Address { Id = 2, City = "City2" }
			]
		};

		var target = new Person
		{
			Name = "Jane",
			Age = 25,
			Addresses =
			[
				new Address { Id = 1, City = "City1-Updated" },
				new Address { Id = 3, City = "City3" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Equal(5, differences.Length);
		Assert.Contains(differences, d => d is NameChangedDifference);
		Assert.Contains(differences, d => d is AgeChangedDifference);
		Assert.Contains(differences, d => d is AddressCityChangedDifference);
		Assert.Contains(differences, d => d is AddressRemovedDifference);
		Assert.Contains(differences, d => d is AddressAddedDifference);
	}

	[Fact]
	public void Compare_MultipleCollectionRules_ShouldDetectAllDifferences()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();

		// Rule 1: Detect additions
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
			addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City));

		// Rule 2: Detect removals
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
				new Address { Id = 1, City = "City1" },
				new Address { Id = 3, City = "City3" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Equal(2, differences.Length);
		Assert.Contains(differences, d => d is AddressRemovedDifference);
		Assert.Contains(differences, d => d is AddressAddedDifference);
	}

	[Fact]
	public void Compare_NoChanges_ShouldReturnEmpty()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRule(
			condition: (s, t) => s.Name != t.Name,
			differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));
		comparer.AddRuleForEach(
			itemsSelector: p => p.Addresses,
			matchingPredicate: (s, t) => s.Id == t.Id,
			addedFactory: (s, t, added) => new AddressAddedDifference(added.Id, added.City),
			removedFactory: (s, t, removed) => new AddressRemovedDifference(removed.Id));

		var source = new Person
		{
			Name = "John",
			Addresses =
			[
				new Address { Id = 1, City = "City1" }
			]
		};

		var target = new Person
		{
			Name = "John",
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
	public void Compare_IdenticalObjects_ShouldReturnEmpty()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRule(
			condition: (s, t) => s.Name != t.Name,
			differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));
		comparer.AddRule(
			condition: (s, t) => s.Age != t.Age,
			differenceFactory: (s, t) => new AgeChangedDifference(s.Age, t.Age));

		var person = new Person { Name = "John", Age = 30 };

		// Act
		var differences = comparer.Compare(person, person);

		// Assert
		Assert.Empty(differences);
	}

	[Fact]
	public void Compare_CanBeCalledMultipleTimes()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRule(
			condition: (s, t) => s.Name != t.Name,
			differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));

		var source1 = new Person { Name = "John" };
		var target1 = new Person { Name = "Jane" };

		var source2 = new Person { Name = "Alice" };
		var target2 = new Person { Name = "Bob" };

		// Act
		var differences1 = comparer.Compare(source1, target1);
		var differences2 = comparer.Compare(source2, target2);

		// Assert
		Assert.Single(differences1);
		Assert.Single(differences2);

		var diff1 = Assert.IsType<NameChangedDifference>(differences1[0]);
		Assert.Equal("John", diff1.OldName);
		Assert.Equal("Jane", diff1.NewName);

		var diff2 = Assert.IsType<NameChangedDifference>(differences2[0]);
		Assert.Equal("Alice", diff2.OldName);
		Assert.Equal("Bob", diff2.NewName);
	}

	[Fact]
	public void Compare_ComplexNestedScenario_WithMultipleChanges()
	{
		// Arrange
		var comparer = new Comparer<Person, IDifference>();
		comparer.AddRule(
			condition: (s, t) => s.Name != t.Name,
			differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));
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
			Name = "John",
			Addresses =
			[
				new Address { Id = 1, City = "CityA" },
				new Address { Id = 2, City = "CityB" },
				new Address { Id = 3, City = "CityC" }
			]
		};

		var target = new Person
		{
			Name = "John",
			Addresses =
			[
				new Address { Id = 1, City = "CityA-Modified" },
				new Address { Id = 2, City = "CityB" },
				new Address { Id = 4, City = "CityD" }
			]
		};

		// Act
		var differences = comparer.Compare(source, target);

		// Assert
		Assert.Equal(3, differences.Length);
		Assert.Contains(differences, d => d is AddressCityChangedDifference cityDiff && cityDiff.AddressId == 1);
		Assert.Contains(differences, d => d is AddressRemovedDifference removedDiff && removedDiff.AddressId == 3);
		Assert.Contains(differences, d => d is AddressAddedDifference addedDiff && addedDiff.AddressId == 4);
	}
}
