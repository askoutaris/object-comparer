# ObjectComparer

A flexible .NET library for comparing two objects of the same type and extracting differences based on configurable rules.

[![NuGet](https://img.shields.io/nuget/v/ObjectComparer.svg)](https://www.nuget.org/packages/ObjectComparer/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Overview

ObjectComparer allows you to define custom comparison rules for objects and their nested collections. Instead of prescribing specific difference types, the library is fully generic - you define your own difference classes that fit your domain needs.

**Key Features:**
- Rule-based comparison system with fluent API
- Simple property comparisons
- Collection comparisons with add/remove/change detection
- Two collection comparison strategies: flexible predicates or optimized key-based matching
- Recursive comparison for nested collections
- Fully generic - define your own difference types
- Targets .NET Standard 2.0 for broad compatibility
- 100% test coverage
- Comprehensive XML documentation

## Installation

```bash
dotnet add package ObjectComparer
```

Or via NuGet Package Manager:
```
Install-Package ObjectComparer
```

## Quick Start

```csharp
using ObjectComparer;

// Define your difference type (interface or abstract class)
public interface IDifference { }

public class NameChanged : IDifference
{
    public string OldName { get; }
    public string NewName { get; }

    public NameChanged(string oldName, string newName)
    {
        OldName = oldName;
        NewName = newName;
    }
}

// Create a comparer
IComparer<Person, IDifference> comparer = new Comparer<Person, IDifference>();

// Add comparison rules
comparer.AddRule(
    condition: (source, target) => source.Name != target.Name,
    differenceFactory: (source, target) => new NameChanged(source.Name, target.Name));

// Compare objects
var person1 = new Person { Name = "John" };
var person2 = new Person { Name = "Jane" };
IDifference[] differences = comparer.Compare(person1, person2);
```

## Usage Guide

### Simple Property Comparisons

Use `AddRule()` to compare simple properties:

```csharp
comparer.AddRule(
    condition: (source, target) => source.Age != target.Age,
    differenceFactory: (source, target) => new AgeChanged(source.Age, target.Age));

comparer.AddRule(
    condition: (source, target) => source.Email != target.Email,
    differenceFactory: (source, target) => new EmailChanged(source.Email, target.Email));
```

Rules are evaluated in order and multiple rules can produce differences for the same comparison.

### Collection Comparisons

ObjectComparer provides two approaches for comparing collections, each optimized for different scenarios:

#### Option 1: Key-Based Matching (Recommended for Performance)

Use when items have unique keys (like Id properties). This approach uses dictionary-based lookups for **O(n) performance**.

```csharp
comparer.AddRuleForEach(
    itemsSelector: person => person.Addresses,
    keySelector: address => address.Id,  // Must be unique!
    addedFactory: (source, target, added) => new AddressAdded(added.Id, added.City),
    removedFactory: (source, target, removed) => new AddressRemoved(removed.Id),
    configureComparer: itemComparer => itemComparer
        .AddRule(
            condition: (sourceAddr, targetAddr) => sourceAddr.City != targetAddr.City,
            differenceFactory: (sourceAddr, targetAddr) =>
                new AddressCityChanged(sourceAddr.Id, sourceAddr.City, targetAddr.City)));
```

**Performance:** O(n) - uses dictionaries for efficient lookups
**Requirement:** Keys must be unique within each collection (throws `ArgumentException` if duplicates found)
**Best for:** Collections with natural unique identifiers (Id, Key, Code, etc.)

#### Option 2: Predicate-Based Matching (For Complex Logic)

Use when matching requires complex logic beyond simple key equality. This approach uses **O(n²) performance** but supports any matching logic.

```csharp
comparer.AddRuleForEach(
    itemsSelector: person => person.Addresses,
    matchingPredicate: (sourceAddr, targetAddr) =>
        sourceAddr.Id == targetAddr.Id,  // Can be any complex logic
    addedFactory: (source, target, added) => new AddressAdded(added.Id, added.City),
    removedFactory: (source, target, removed) => new AddressRemoved(removed.Id),
    configureComparer: itemComparer => itemComparer
        .AddRule(
            condition: (sourceAddr, targetAddr) => sourceAddr.City != targetAddr.City,
            differenceFactory: (sourceAddr, targetAddr) =>
                new AddressCityChanged(sourceAddr.Id, sourceAddr.City, targetAddr.City)));
```

**Performance:** O(n²) - iterates through items to find matches
**Requirement:** Predicate must uniquely identify items (throws `InvalidOperationException` if multiple matches found)
**Best for:**
- Composite keys: `(s, t) => s.FirstName == t.FirstName && s.LastName == t.LastName`
- Fuzzy matching: `(s, t) => s.Name.Equals(t.Name, StringComparison.OrdinalIgnoreCase)`
- Complex business logic for matching

### Nested Comparers

The `configureComparer` parameter allows recursive comparison of matched items:

```csharp
comparer.AddRuleForEach(
    itemsSelector: company => company.Departments,
    keySelector: dept => dept.Id,
    configureComparer: deptComparer => deptComparer
        // Compare department properties
        .AddRule(
            condition: (s, t) => s.Name != t.Name,
            differenceFactory: (s, t) => new DepartmentNameChanged(s.Name, t.Name))
        // Recursively compare employees within each department
        .AddRuleForEach(
            itemsSelector: dept => dept.Employees,
            keySelector: emp => emp.Id,
            configureComparer: empComparer => empComparer
                .AddRule(
                    condition: (s, t) => s.Salary != t.Salary,
                    differenceFactory: (s, t) => new SalaryChanged(s.Id, s.Salary, t.Salary))));
```

## Complete Example

```csharp
using ObjectComparer;

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public Address[] Addresses { get; set; } = Array.Empty<Address>();
}

public class Address
{
    public int Id { get; set; }
    public string City { get; set; } = string.Empty;
}

// Define your difference types
public interface IDifference { }

public class NameChanged : IDifference
{
    public string OldName { get; }
    public string NewName { get; }

    public NameChanged(string oldName, string newName)
    {
        OldName = oldName;
        NewName = newName;
    }
}

public class AgeChanged : IDifference
{
    public int OldAge { get; }
    public int NewAge { get; }

    public AgeChanged(int oldAge, int newAge)
    {
        OldAge = oldAge;
        NewAge = newAge;
    }
}

public class AddressAdded : IDifference
{
    public int AddressId { get; }
    public string City { get; }

    public AddressAdded(int addressId, string city)
    {
        AddressId = addressId;
        City = city;
    }
}

public class AddressRemoved : IDifference
{
    public int AddressId { get; }

    public AddressRemoved(int addressId)
    {
        AddressId = addressId;
    }
}

public class AddressCityChanged : IDifference
{
    public int AddressId { get; }
    public string OldCity { get; }
    public string NewCity { get; }

    public AddressCityChanged(int addressId, string oldCity, string newCity)
    {
        AddressId = addressId;
        OldCity = oldCity;
        NewCity = newCity;
    }
}

// Usage
class Program
{
    static void Main()
    {
        var person1 = new Person
        {
            Name = "John",
            Age = 30,
            Addresses = new[]
            {
                new Address { Id = 1, City = "New York" },
                new Address { Id = 2, City = "Boston" }
            }
        };

        var person2 = new Person
        {
            Name = "John",
            Age = 31,
            Addresses = new[]
            {
                new Address { Id = 1, City = "Los Angeles" },
                new Address { Id = 3, City = "Chicago" }
            }
        };

        // Create comparer with rules
        IComparer<Person, IDifference> comparer = new Comparer<Person, IDifference>();

        comparer.AddRule(
            condition: (source, target) => source.Name != target.Name,
            differenceFactory: (source, target) => new NameChanged(source.Name, target.Name));

        comparer.AddRule(
            condition: (source, target) => source.Age != target.Age,
            differenceFactory: (source, target) => new AgeChanged(source.Age, target.Age));

        // Use key-based matching for O(n) performance
        comparer.AddRuleForEach(
            itemsSelector: person => person.Addresses,
            keySelector: address => address.Id,
            addedFactory: (source, target, added) => new AddressAdded(added.Id, added.City),
            removedFactory: (source, target, removed) => new AddressRemoved(removed.Id),
            configureComparer: itemComparer => itemComparer
                .AddRule(
                    condition: (sourceAddr, targetAddr) => sourceAddr.City != targetAddr.City,
                    differenceFactory: (sourceAddr, targetAddr) =>
                        new AddressCityChanged(sourceAddr.Id, sourceAddr.City, targetAddr.City)));

        // Compare
        IDifference[] differences = comparer.Compare(person1, person2);

        // Results:
        // - AgeChanged(30, 31)
        // - AddressRemoved(2)
        // - AddressAdded(3, "Chicago")
        // - AddressCityChanged(1, "New York", "Los Angeles")
    }
}
```

## Performance Considerations

| Collection Size | Key-Based (`keySelector`) | Predicate-Based (`matchingPredicate`) |
|----------------|---------------------------|---------------------------------------|
| 10 items       | ~instant                  | ~instant                              |
| 100 items      | ~instant                  | ~instant                              |
| 1,000 items    | < 1ms                     | ~10ms                                 |
| 10,000 items   | ~10ms                     | ~1000ms (1 second)                    |

**Recommendation:** Use `keySelector` whenever possible. Only use `matchingPredicate` when you need complex matching logic that cannot be expressed as a simple key.

## Building from Source

```bash
# Build the solution
dotnet build ObjectComparer.sln

# Run tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run the demo workbench
dotnet run --project Workbench/Workbench.csproj

# Pack for NuGet
dotnet pack ObjectComparer/ObjectComparer.csproj -c Release
```

## How It Works

The library uses a rule-based comparison pattern:

1. You create a `Comparer<TType, TDiff>` instance
2. You add rules via `AddRule()` and `AddRuleForEach()`
3. Each rule implements the internal `IRule` interface
4. When you call `Compare()`, all rules are executed in order
5. All differences are aggregated and returned as an array

The `Comparer` class is split across multiple partial class files:
- `Comparer.cs` - Main API
- `Comparer.Rule.cs` - Simple property comparison rules
- `Comparer.RuleForEach.cs` - Predicate-based collection rules (O(n²))
- `Comparer.RuleForEachWithKey.cs` - Key-based collection rules (O(n))
- `Comparer.IRule.cs` - Internal rule interface

## License

MIT License - see LICENSE file for details

## Author

Alkiviadis Skoutaris

## Repository

https://github.com/askoutaris/object-comparer

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
