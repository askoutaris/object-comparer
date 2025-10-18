namespace Workbench
{
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
