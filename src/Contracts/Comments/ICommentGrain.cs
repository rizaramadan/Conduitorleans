namespace Contracts.Comments
{
    using System.Threading.Tasks;
    using Orleans;

    public interface ICommentGrain : IGrainWithIntegerCompoundKey
    {
        Task<(Comment Comment, Error Error)> Get();
        Task<Error> Set(Comment comment);
    }
}
