# ObjectComparer

A flexible .NET library for comparing two objects of the same type and extracting differences based on configurable rules.

## Overview

ObjectComparer allows you to define custom comparison rules for objects and their nested collections. Instead of prescribing specific difference types, the library is fully generic - you define your own difference classes that fit your domain needs.

**Key Features:**
- Rule-based comparison system with fluent API
- Support for simple property comparisons
- Support for collection comparisons with add/remove/change detection
- Recursive comparison for nested collections
- Fully generic - define your own difference types
- Targets .NET Standard 2.0 for broad compatibility

## Installation

```
dotnet add package ObjectComparer
```

Or via NuGet Package Manager:
```
Install-Package ObjectComparer
```

## Usage

### Basic Example

```csharp
using System;
using ObjectComparer;

namespace Workbench
{
	class Program
	{
		static void Main(string[] args)
		{
			var person1 = new Person
			{
				Name = "Name",
				Addresses = new Address[] {
					new Address{ Id = 1, City = "City1" },
					new Address{ Id = 3, City = "City3" },
					new Address{ Id = 4, City = "City4" },
				}
			};

			var person2 = new Person
			{
				Name = "NameLonger",
				Addresses = new Address[] {
					new Address{ Id = 2, City = "City2" },
					new Address{ Id = 3, City = "City33" },
					new Address{ Id = 4, City = "City4" },
				}
			};

			IComparer<Person, DifferenceBase> comparer = new Comparer<Person, DifferenceBase>();

			// Add a simple property comparison rule
			comparer.AddRule(
				condition: (source, target) => source.Name != target.Name,
				differenceFactory: (source, target) => new GenericDifference($"The new name is {target.Name}"));

			// Add another rule with more specific condition
			comparer.AddRule(
				condition: (source, target) => source.Name != target.Name && source.Name.Length < target.Name.Length,
				differenceFactory: (source, target) => new LongerNameDifference(source.Name, target.Name));

			// Add a collection comparison rule with nested comparer
			comparer.AddRuleForEach(
			 	itemsSelector: person => person.Addresses,
				matchingPredicate: (sourceAddress, targetAddress) => sourceAddress.Id == targetAddress.Id,
				addedFactory: (source, target, targetAddressAdded) => new GenericDifference($"Address added addressId: {targetAddressAdded.Id} city: {targetAddressAdded.City}"),
				removedFactory: (source, target, targetAddressRemoved) => new GenericDifference($"Address removed addressId: {targetAddressRemoved.Id} city: {targetAddressRemoved.City}"),
				configureComparer: itemComparer => itemComparer
					.AddRule(
						condition: (sourceAddress, targetAddress) => sourceAddress.City != targetAddress.City,
						differenceFactory: (sourceAddress, targetAddress) => new GenericDifference($"New city name is {sourceAddress.City} for id {sourceAddress.Id}"))
				);

			DifferenceBase[] differences = comparer.Compare(person1, person2);

			foreach (var dif in differences)
				Console.WriteLine(dif.ToString());

			Console.ReadLine();
		}
	}

	// Define your own difference types
	public abstract class DifferenceBase
	{
	}

	public class GenericDifference : DifferenceBase
	{
		public string Message { get; }

		public GenericDifference(string message)
		{
			Message = message;
		}

		public override string ToString()
		{
			return Message;
		}
	}

	public class LongerNameDifference : DifferenceBase
	{
		public string OldName { get; }
		public string NewName { get; }

		public LongerNameDifference(string oldName, string newName)
		{
			OldName = oldName;
			NewName = newName;
		}

		public override string ToString()
		{
			return $"Name \"{NewName}\" is bigger than \"{OldName}\"";
		}
	}

	public class Person
	{
		public string Name { get; set; }
		public Address[] Addresses { get; set; }
	}

	public class Address
	{
		public int Id { get; set; }
		public string City { get; set; }
	}
}
```

## How It Works

### Simple Rules

Use `AddRule()` to compare simple properties:

```csharp
comparer.AddRule(
    condition: (source, target) => source.PropertyName != target.PropertyName,
    differenceFactory: (source, target) => new MyDifference("Property changed")
);
```

### Collection Rules

Use `AddRuleForEach()` to compare collections:

```csharp
comparer.AddRuleForEach(
    itemsSelector: obj => obj.Collection,           // Select the collection to compare
    matchingPredicate: (s, t) => s.Id == t.Id,      // How to match items (must be unique!)
    addedFactory: (s, t, added) => new MyDiff(),    // Factory for added items
    removedFactory: (s, t, removed) => new MyDiff(), // Factory for removed items
    configureComparer: nested => nested.AddRule(...) // Optional: compare matched items
);
```

**Important:** The `matchingPredicate` must uniquely identify items (like a primary key). The library uses `SingleOrDefault` internally to ensure uniqueness.

### Rule Evaluation

- Rules are evaluated in the order they are added
- Multiple rules can produce differences for the same comparison
- All differences are aggregated into a single array result

## Building from Source

Build the solution:
```bash
dotnet build ObjectComparer.sln
```

Run the demo workbench:
```bash
dotnet run --project Workbench/Workbench.csproj
```

Pack for NuGet:
```bash
dotnet pack ObjectComparer/ObjectComparer.csproj -c Release
```

## License

MIT License - see the project file for details

## Author

Alkiviadis Skoutaris

## Repository

https://github.com/askoutaris/object-comparer
