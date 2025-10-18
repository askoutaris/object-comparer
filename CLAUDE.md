# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ObjectComparer is a .NET library that compares two objects of the same type and extracts differences based on configurable rules. The library is designed to be generic and flexible, allowing consumers to define custom difference types.

Target framework: .NET Standard 2.0 (C# 9)

## Project Structure

- `ObjectComparer/` - The main library project (NuGet package, .NET Standard 2.0)
- `Workbench/` - Console application for testing/demonstration purposes (.NET Core 3.1)
- `Tests/` - xUnit test project (.NET 8.0)

## Build and Development Commands

Build the solution:
```
dotnet build ObjectComparer.sln
```

Build in Release mode (for packaging):
```
dotnet build ObjectComparer.sln -c Release
```

Run the Workbench demo:
```
dotnet run --project Workbench/Workbench.csproj
```

Pack the library for NuGet distribution:
```
dotnet pack ObjectComparer/ObjectComparer.csproj -c Release
```

Run all tests:
```
dotnet test
```

Run tests in a specific project:
```
dotnet test Tests/Tests.csproj
```

## Architecture

### Core Design Pattern

The library uses a **rule-based comparison system** with two main rule types:

1. **Simple Rules** (`Rule` class in `Comparer.Rule.cs`): Compare top-level properties of objects
   - Takes a condition function `(source, target) => bool`
   - Takes a difference factory function `(source, target) => TDiff`
   - Returns a difference object when condition is met

2. **Collection Rules** (`RuleForEach<TItem>` class in `Comparer.RuleForEach.cs`): Compare collections within objects
   - Selects a collection from both source and target objects
   - Uses a matching predicate to pair items between collections
   - Detects added items, removed items, and changed items
   - Can recursively apply nested comparers to matched items
   - **Important**: Uses `SingleOrDefault` with the matching predicate to ensure uniqueness - the predicate must identify items uniquely (like a key)

### Key Classes

- `Comparer<TType, TDiff>`: Main entry point implementing `IComparer<TType, TDiff>`
  - Maintains a list of `IRule` instances
  - `AddRule()`: Adds simple comparison rules
  - `AddRuleForEach()`: Adds collection comparison rules with optional nested comparer
  - `Compare()`: Executes all rules and returns aggregated differences

- `IRule` (internal interface): Common interface for all rule types
  - Single method: `TDiff[] Compare(TType source, TType target)`

### Partial Class Structure

The `Comparer<TType, TDiff>` class is split across multiple files using partial classes:
- `Comparer.cs`: Main class definition and public API
- `Comparer.IRule.cs`: Internal `IRule` interface definition
- `Comparer.Rule.cs`: Simple rule implementation
- `Comparer.RuleForEach.cs`: Collection rule implementation

### Generic Difference Pattern

The library does NOT define concrete difference types. Instead:
- Consumers define their own difference base class or interface (e.g., `IDifference` interface or `DifferenceBase` abstract class)
- Consumers create specific difference types that implement/inherit from this base
- The comparer is instantiated as `Comparer<TObjectType, TDifferenceType>`
- Rules provide factory functions that create the appropriate difference instances

See README.md for a complete usage example demonstrating this pattern with an `IDifference` interface.

## Important Implementation Notes

- `RuleForEach` executes its matching predicate using `SingleOrDefault()` to ensure each item has at most one match
- Rules are evaluated in the order they are added
- Multiple rules can produce differences for the same comparison
- All differences are aggregated and returned as an array
- The library targets .NET Standard 2.0 for broad compatibility

## Repository Information

- GitHub: https://github.com/askoutaris/object-comparer
- Author: Alkiviadis Skoutaris
- License: MIT
- Current Version: 1.1.0
