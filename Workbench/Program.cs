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

			IComparer<Person, IDifference> comparer = new Comparer<Person, IDifference>();

			comparer.AddRule(
				condition: (source, target) => source.Name != target.Name,
				differenceFactory: (source, target) => new GenericDifference($"The new name is {target.Name}"));

			comparer.AddRule(
				condition: (source, target) => source.Name != target.Name && source.Name.Length < target.Name.Length,
				differenceFactory: (source, target) => new LongerNameDifference(source.Name, target.Name));

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

			IDifference[] differences = comparer.Compare(person1, person2);

			foreach (var dif in differences)
				Console.WriteLine(dif.ToString());

			Console.ReadLine();
		}
	}

	public interface IDifference
	{
	}

	public class GenericDifference : IDifference
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

	public class LongerNameDifference : IDifference
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
