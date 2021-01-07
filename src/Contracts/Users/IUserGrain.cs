using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Contracts.Users
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<(bool, Error)> HasRegistered();
        Task<Error> Register(string email, string password);
        Task<(string Email,Error Error)> GetEmail();
        Task<Error> Login(string email, string password);
    }
}
