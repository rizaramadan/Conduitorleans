using Orleans;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IHello : IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);
    }
}
