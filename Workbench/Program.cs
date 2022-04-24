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
					new Address{ Id = 5, City = "City5" }
				}
			};

			var person2 = new Person
			{
				Name = "Name2",
				Addresses = new Address[] {
					new Address{ Id = 1, City = "City1.1" },
					new Address{ Id = 2, City = "City2" }
				}
			};

			IComparer<Person> comparer = new Comparer<Person>();

			comparer.AddRule(
				condition: (source, target) => source.Name != target.Name,
				differenceFactory: (source, target) => new GenericDifference($"The new name is {target.Name}"));

			comparer.AddRule(
				condition: (source, target) => source.Name != target.Name && source.Name.Length < target.Name.Length,
				differenceFactory: (source, target) => new BiggerNameDifference(source.Name, target.Name));

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

	public class BiggerNameDifference : IDifference
	{
		public string OldName { get; }
		public string NewName { get; }

		public BiggerNameDifference(string oldName, string newName)
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
