using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace GrainInterfaces.Security
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<(bool, IError)> HasRegistered();
        Task<IError> Register(string email, string password);
        Task<IError> Login(string email, string password);
    }



}
