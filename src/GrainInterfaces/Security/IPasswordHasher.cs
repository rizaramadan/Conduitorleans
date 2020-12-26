using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace GrainInterfaces.Security
{
    public interface IPasswordHasher : IGrainWithStringKey
    {
        Task<byte[]> Hash(string password, byte[] salt);
    }
}
