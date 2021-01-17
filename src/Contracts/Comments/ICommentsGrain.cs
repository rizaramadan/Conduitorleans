namespace Contracts.Comments
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Orleans;

    public interface ICommentsGrain : IGrainWithStringKey
    {
        Task<(long Id, Error Error)> AddComment(string currentUser, string slug, Comment comment);
        Task<Error> RemoveComment(string currentUser, long id, string slug);
        Task<(List<Comment> Comments, Error Error)> Get(string currentUser, string slug);
    }
}
