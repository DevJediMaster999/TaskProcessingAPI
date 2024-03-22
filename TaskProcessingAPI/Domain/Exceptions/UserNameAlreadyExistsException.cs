namespace TaskProcessingAPI.Domain.Exceptions
{
    public class UserNameAlreadyExistsException(string name) : Exception
    {
        public string Name => name;
    }
}
