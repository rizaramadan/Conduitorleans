using System;
using System.Collections.Generic;
using System.Text;

namespace GrainInterfaces
{
    public interface IError
    {
        public Guid Code { get; }
        public string Message { get; }
    }

    public class Error : IError
    {
        public static readonly Error None = new Error();

        public Guid Code { get => Guid.Empty; }
        public string Message { get => nameof(None); }

        public override bool Equals(object obj) =>
            obj is IError error && Code.Equals(error.Code);

        public override int GetHashCode() => Code.GetHashCode();
    }

    public static class IErrorExtension
    {
        public static bool Exist(this IError error) => error != Error.None;
    }
}
