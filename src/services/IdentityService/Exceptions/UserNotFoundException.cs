namespace IdentityService.Exceptions
{
    public class UserNotFoundException : IdentityDomainException
    {
        public UserNotFoundException(string identifier) 
            : base($"User with identifier '{identifier}' was not found.") { }
    }
}
