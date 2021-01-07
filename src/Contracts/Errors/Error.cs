using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public class Error
    {
        public static readonly Error None = new Error(Guid.Empty, nameof(None));

        public Guid Code { get; }
        public string Message { get; }

        public Error(Guid c, string m)
        {
            Code = c;
            Message = m;
        }

        public Error(string code, string message)
        {
            Code = Guid.TryParse(code, out var codeGuid)
                ? codeGuid
                : throw new InvalidCastException();
            Message = message;
        }

        public override bool Equals(object obj) =>
            obj is Error error && Code.Equals(error.Code);

        public override int GetHashCode() => Code.GetHashCode();
    }

    public static class IErrorExtension
    {
        public static bool Exist(this Error error) => !error.Equals(Error.None);
    }
}
