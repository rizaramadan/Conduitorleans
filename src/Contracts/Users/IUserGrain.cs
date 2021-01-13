namespace Contracts.Users
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IUserGrain : IGrainWithStringKey
    {
        Task<(bool, Error)> HasRegistered();
        Task<Error> Register(string email, string password);
        Task<Error> Login(string email, string password);
        Task<Error> Update(UpdateUser user);
        Task<(string Email,Error Error)> GetEmail();
        Task<(User User, Error Error)> Get();
    }
}
