namespace WorkoutManager.Business.Exceptions
{
    public class DuplicateEntryException : Exception
    {
        public DuplicateEntryException(string message) : base(message)
        {
        }

        public DuplicateEntryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
