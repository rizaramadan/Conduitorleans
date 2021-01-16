namespace Grains.Comments
{
    using Contracts;
    using Contracts.Comments;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class CommentGrain : Grain, ICommentGrain
    {
        public Task<(Comment Comment, Error Error)> Get()
        {
            throw new NotImplementedException();
        }

        public Task<Error> Set(Comment comment)
        {
            throw new NotImplementedException();
        }
    }
}
