using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public interface IError
    {
        public Guid Code { get; }
        public string Message { get; }
    }

    public class Error : IError
    {
        public static readonly Error None = new Error(Guid.Empty, nameof(None));

        public Error(Guid code, string message)
        {
            Code = code;
            Message = message;
        }

        public Error(string code, string message)
        {
            Code = Guid.TryParse(code, out var codeGuid)
                ? codeGuid
                : throw new InvalidCastException();
            Message = message;
        }

        public Guid Code { get; }
        public string Message { get; }

        public override bool Equals(object obj) =>
            obj is IError error && Code.Equals(error.Code);

        public override int GetHashCode() => Code.GetHashCode();
    }

    public static class IErrorExtension
    {
        public static bool Exist(this IError error) => !error.Equals(Error.None);
    }
}
