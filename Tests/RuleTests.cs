using ObjectComparer;

namespace Tests;

public class RuleTests
{
    [Fact]
    public void AddRule_WhenConditionIsTrue_ShouldReturnDifference()
    {
        // Arrange
        var comparer = new Comparer<Person, IDifference>();
        comparer.AddRule(
            condition: (s, t) => s.Name != t.Name,
            differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));

        var source = new Person { Name = "John" };
        var target = new Person { Name = "Jane" };

        // Act
        var differences = comparer.Compare(source, target);

        // Assert
        Assert.Single(differences);
        var diff = Assert.IsType<NameChangedDifference>(differences[0]);
        Assert.Equal("John", diff.OldName);
        Assert.Equal("Jane", diff.NewName);
    }

    [Fact]
    public void AddRule_WhenConditionIsFalse_ShouldReturnNoDifferences()
    {
        // Arrange
        var comparer = new Comparer<Person, IDifference>();
        comparer.AddRule(
            condition: (s, t) => s.Name != t.Name,
            differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));

        var source = new Person { Name = "John" };
        var target = new Person { Name = "John" };

        // Act
        var differences = comparer.Compare(source, target);

        // Assert
        Assert.Empty(differences);
    }

    [Fact]
    public void AddRule_MultipleRules_ShouldReturnAllMatchingDifferences()
    {
        // Arrange
        var comparer = new Comparer<Person, IDifference>();
        comparer.AddRule(
            condition: (s, t) => s.Name != t.Name,
            differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));
        comparer.AddRule(
            condition: (s, t) => s.Age != t.Age,
            differenceFactory: (s, t) => new AgeChangedDifference(s.Age, t.Age));

        var source = new Person { Name = "John", Age = 30 };
        var target = new Person { Name = "Jane", Age = 25 };

        // Act
        var differences = comparer.Compare(source, target);

        // Assert
        Assert.Equal(2, differences.Length);
        Assert.Contains(differences, d => d is NameChangedDifference);
        Assert.Contains(differences, d => d is AgeChangedDifference);
    }

    [Fact]
    public void AddRule_MultipleRulesPartialMatch_ShouldReturnOnlyMatchingDifferences()
    {
        // Arrange
        var comparer = new Comparer<Person, IDifference>();
        comparer.AddRule(
            condition: (s, t) => s.Name != t.Name,
            differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));
        comparer.AddRule(
            condition: (s, t) => s.Age != t.Age,
            differenceFactory: (s, t) => new AgeChangedDifference(s.Age, t.Age));

        var source = new Person { Name = "John", Age = 30 };
        var target = new Person { Name = "Jane", Age = 30 };

        // Act
        var differences = comparer.Compare(source, target);

        // Assert
        Assert.Single(differences);
        Assert.IsType<NameChangedDifference>(differences[0]);
    }

    [Fact]
    public void AddRule_SupportsMethodChaining()
    {
        // Arrange & Act
        var comparer = new Comparer<Person, IDifference>()
            .AddRule(
                condition: (s, t) => s.Name != t.Name,
                differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name))
            .AddRule(
                condition: (s, t) => s.Age != t.Age,
                differenceFactory: (s, t) => new AgeChangedDifference(s.Age, t.Age));

        var source = new Person { Name = "John", Age = 30 };
        var target = new Person { Name = "Jane", Age = 25 };

        var differences = comparer.Compare(source, target);

        // Assert
        Assert.Equal(2, differences.Length);
    }

    [Fact]
    public void AddRule_ComplexCondition_ShouldEvaluateCorrectly()
    {
        // Arrange
        var comparer = new Comparer<Person, IDifference>();
        comparer.AddRule(
            condition: (s, t) => s.Name != t.Name && s.Name.Length < t.Name.Length,
            differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));

        var source = new Person { Name = "John" };
        var target = new Person { Name = "Jane" };

        // Act
        var differences = comparer.Compare(source, target);

        // Assert
        Assert.Empty(differences); // Same length, so condition is false
    }

    [Fact]
    public void AddRule_ComplexConditionTrue_ShouldReturnDifference()
    {
        // Arrange
        var comparer = new Comparer<Person, IDifference>();
        comparer.AddRule(
            condition: (s, t) => s.Name != t.Name && s.Name.Length < t.Name.Length,
            differenceFactory: (s, t) => new NameChangedDifference(s.Name, t.Name));

        var source = new Person { Name = "John" };
        var target = new Person { Name = "Jonathan" };

        // Act
        var differences = comparer.Compare(source, target);

        // Assert
        Assert.Single(differences);
        var diff = Assert.IsType<NameChangedDifference>(differences[0]);
        Assert.Equal("John", diff.OldName);
        Assert.Equal("Jonathan", diff.NewName);
    }
}
