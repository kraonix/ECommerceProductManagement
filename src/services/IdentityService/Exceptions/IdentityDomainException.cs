using System;

namespace IdentityService.Exceptions
{
    public abstract class IdentityDomainException : Exception
    {
        protected IdentityDomainException(string message) : base(message) { }
    }
}
