namespace Contracts.Comments
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Orleans;

    public interface ICommentsGrain : IGrainWithStringKey
    {
        Task<Error> AddComment(Comment comment);
        Task<Error> RemoveComment(long id);
        Task<(List<Comment> Comments, Error Error)> Get();
    }
}
