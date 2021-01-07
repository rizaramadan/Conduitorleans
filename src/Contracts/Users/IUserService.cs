using System.Threading.Tasks;

namespace Contracts.Users
{
    public interface IUserService
    {
        Task<(string, Error)> GetUsernameByEmail(string email);

        (string Username, Error Error) GetCurrentUsername();
    }
}
