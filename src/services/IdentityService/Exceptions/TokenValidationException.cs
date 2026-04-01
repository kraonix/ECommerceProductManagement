namespace IdentityService.Exceptions
{
    public class TokenValidationException : IdentityDomainException
    {
        public TokenValidationException(string details) 
            : base($"Token validation failed: {details}") { }
    }
}
