namespace Contracts.Users
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IEmailUserGrain : IGrainWithStringKey
    {
        Task<(string, Error)> GetUsername();
        Task<Error> SetUsername(string username);
    }
}
