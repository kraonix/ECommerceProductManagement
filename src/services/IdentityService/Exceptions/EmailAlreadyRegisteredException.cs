namespace IdentityService.Exceptions
{
    public class EmailAlreadyRegisteredException : IdentityDomainException
    {
        public EmailAlreadyRegisteredException(string email) 
            : base($"The email address '{email}' is already in use by another account.") { }
    }
}
