using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Errors
{
    public class UserRegisteredError : Error
    {
        static readonly Guid TheCode = Guid.Parse("e119c62f-c276-4799-a6a0-fbc43da87c2b");
        static readonly string TheMessage = "user already registered";
        public UserRegisteredError() : base(TheCode, TheMessage)
        { }
    }
}
