using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Contracts.Users
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<(bool, IError)> HasRegistered();
        Task<IError> Register(string email, string password);
        Task<(string Email,IError Error)> GetEmail();
        Task<IError> Login(string email, string password);
    }
}
