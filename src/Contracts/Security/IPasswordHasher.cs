using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Contracts.Security
{
    public interface IPasswordHasher : IGrainWithIntegerKey
    {
        Task<byte[]> Hash(string password, byte[] salt);
    }
}
