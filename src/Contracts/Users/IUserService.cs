using System.Threading.Tasks;

namespace Contracts.Users
{
    public interface IUserService
    {
        (string Username, Error Error) GetCurrentUsername();
    }
}
