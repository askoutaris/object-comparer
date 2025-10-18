namespace Workbench
{
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
}
