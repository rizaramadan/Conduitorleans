namespace Grains.Comments
{
    using Contracts;
    using Contracts.Comments;
    using Orleans;
    using Orleans.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using PersistenceState = Orleans.Runtime.IPersistentState<System.Collections.Generic.List<long>>;

    public class CommentsGrain : Grain, ICommentsGrain
    {
        private readonly PersistenceState _comments;
        private readonly IGrainFactory _factory;

        public CommentsGrain(
            [PersistentState(nameof(CommentsGrain), Constants.GrainStorage)] PersistenceState s,
            IGrainFactory f
        )
        {
            _comments = s;
            _factory = f;
        }

        public async Task<Error> AddComment(Comment comment)
        {
            long commentId;
            if (_comments.State == null)
            {
                _comments.State = new List<long>(1);
                commentId = 1L;
            }
            else
            {
                commentId = _comments.State.Last() + 1;
            }
            comment.Id = commentId;
            _comments.State.Add(comment.Id);
            var commentGrain = _factory.GetGrain<ICommentGrain>(commentId,this.GetPrimaryKeyString());
            var taskComment = commentGrain.Set(comment);
            var taskComments = _comments.WriteStateAsync();
            await Task.WhenAll(taskComment, taskComments);
            return Error.None;
            
        }

        public async Task<Error> RemoveComment(long id)
        {
            if (_comments.State == null) 
            {
                return Error.None;
            }
            
            _comments.State.Remove(id);
            await _comments.WriteStateAsync();
            return Error.None;
        }

        public async Task<(List<Comment> Comments, Error Error)> Get()
        {
            var tasks = new List<Task<(Comment Comment, Error Error)>>();
            foreach (var each in _comments.State ?? Enumerable.Empty<long>())
            {
                var commentGrain = _factory.GetGrain<ICommentGrain>(each, this.GetPrimaryKeyString());
                tasks.Add(commentGrain.Get());
            }
            var result = (await Task.WhenAll(tasks)).Select(x => x.Comment).ToList();
            return (result, Error.None);
        }
    }
}
