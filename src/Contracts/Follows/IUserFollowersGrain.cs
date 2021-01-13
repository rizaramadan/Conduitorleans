namespace Contracts.Follows
{
    using Orleans;
    using System.Threading.Tasks;

    public interface IUserFollowersGrain : IGrainWithStringKey
    {
        Task<Error> AddFollower(string username);
        Task<Error> RemoveFollower(string username);
    }
}
