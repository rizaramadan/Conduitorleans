namespace Contracts.Follows
{
    using Orleans;
    using System.Threading.Tasks;

    public interface IUserFollowingGrain : IGrainWithStringKey
    {
        Task<Error> Follow(string username);
        Task<Error> Unfollow(string username);
    }
}
