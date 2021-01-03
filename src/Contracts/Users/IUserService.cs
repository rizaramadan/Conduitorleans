using System.Threading.Tasks;

namespace Contracts.Users
{
    public interface IUserService
    {
        Task<(string, IError)> GetUsernameByEmail(string email);

        (string, IError) GetCurrentUsername();
    }
}
