namespace ObjectComparer
{
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
}
