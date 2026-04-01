namespace IdentityService.Exceptions
{
    public class InvalidCredentialsException : IdentityDomainException
    {
        public InvalidCredentialsException() 
            : base("Invalid login credentials provided.") { }
    }
}
