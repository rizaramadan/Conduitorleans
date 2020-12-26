using Orleans;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public interface IHello : IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);
    }
}
