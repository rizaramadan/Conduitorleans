using System.Threading.Tasks;

namespace GrainInterfaces.Services
{
    public interface IUserService
    {
        Task<(string, IError)> GetUsernameByEmail(string email);

        (string, IError) GetCurrentUsername();
    }
}
