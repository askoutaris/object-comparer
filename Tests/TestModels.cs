namespace Tests;

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

public interface IDifference { }

public class NameChangedDifference : IDifference
{
	public string OldName { get; }
	public string NewName { get; }

	public NameChangedDifference(string oldName, string newName)
	{
		OldName = oldName;
		NewName = newName;
	}
}

public class AgeChangedDifference : IDifference
{
	public int OldAge { get; }
	public int NewAge { get; }

	public AgeChangedDifference(int oldAge, int newAge)
	{
		OldAge = oldAge;
		NewAge = newAge;
	}
}

public class AddressAddedDifference : IDifference
{
	public int AddressId { get; }
	public string City { get; }

	public AddressAddedDifference(int addressId, string city)
	{
		AddressId = addressId;
		City = city;
	}
}

public class AddressRemovedDifference : IDifference
{
	public int AddressId { get; }

	public AddressRemovedDifference(int addressId)
	{
		AddressId = addressId;
	}
}

public class AddressCityChangedDifference : IDifference
{
	public int AddressId { get; }
	public string OldCity { get; }
	public string NewCity { get; }

	public AddressCityChangedDifference(int addressId, string oldCity, string newCity)
	{
		AddressId = addressId;
		OldCity = oldCity;
		NewCity = newCity;
	}
}
