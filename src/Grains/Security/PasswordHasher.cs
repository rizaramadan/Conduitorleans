using GrainInterfaces.Security;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Grains.Security
{
    [StatelessWorker]
    [Reentrant]
    public class PasswordHasher : Grain, IPasswordHasher
    {
        private static readonly HMACSHA512 x = new HMACSHA512(Encoding.UTF8.GetBytes("Conduitorleans"));

        public async Task<byte[]> Hash(string password, byte[] salt)
        {
            var bytes = Encoding.UTF8.GetBytes(password);

            var allBytes = new byte[bytes.Length + salt.Length];
            Buffer.BlockCopy(bytes, 0, allBytes, 0, bytes.Length);
            Buffer.BlockCopy(salt, 0, allBytes, bytes.Length, salt.Length);

            return await Task.FromResult(x.ComputeHash(allBytes));
        }
    }
}
