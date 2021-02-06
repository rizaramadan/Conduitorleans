namespace Contracts
{
    using Orleans;
    using System.Threading.Tasks;

    public interface ICounterGrain : IGrainWithStringKey
    {
        Task<Error> Increement();
        Task<Error> Decreement();
        Task<ulong> Get();
    }
}
