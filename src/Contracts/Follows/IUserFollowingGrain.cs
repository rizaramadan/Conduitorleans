namespace Contracts.Follows
{
    using Orleans;
    using System.Threading.Tasks;

    public interface IUserFollowingGrain : IGrainWithStringKey
    {
        Task<bool> IsFollow(string username);
        Task<Error> Follow(string username);
        Task<Error> Unfollow(string username);
    }
}
