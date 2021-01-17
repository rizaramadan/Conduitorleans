namespace Grains.Comments
{
    using Contracts;
    using Contracts.Comments;
    using Contracts.Users;
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

        public async Task<(long Id, Error Error)> AddComment(string username, string slug, Comment comment)
        {
            long commentId;
            if (_comments.State == null)
            {
                _comments.State = new List<long>(1);
                commentId = 1L;
            }
            else
            {
                commentId = _comments.State.Count == 0 ?
                        1
                        : _comments.State.Last() + 1;
            }
            comment.Id = commentId;
            comment.Author = new Profile { Username = username };
            comment.CreatedAt = DateTime.Now;
            comment.UpdatedAt = comment.CreatedAt;
            _comments.State.Add(comment.Id);
            var commentGrain = _factory.GetGrain<ICommentGrain>(commentId,slug);
            var taskComment = commentGrain.Set(comment);
            var taskComments = _comments.WriteStateAsync();
            await Task.WhenAll(taskComment, taskComments);
            return (commentId, Error.None);
            
        }

        public async Task<Error> RemoveComment(string username, long id, string slug)
        {
            if (_comments.State == null)
            {
                return Contracts.Error.None;
            }
            var commentGrain = _factory.GetGrain<ICommentGrain>(id, slug);
            (Comment Comment, Error Error) = await commentGrain.Get(username);
            if (Error.Exist())
            {
                return Error;
            }

            if (!Comment.Author.Username.Equals(username, StringComparison.Ordinal)) 
            {
                return new Error("03A8ED41-5D30-494F-B149-71AF368742DF", "äuthor is not deleter");
            }

            _comments.State.Remove(id);
            await _comments.WriteStateAsync();
            return Error.None;
        }

        public async Task<(List<Comment> Comments, Error Error)> Get(string currentUser, string slug)
        {
            var tasks = new List<Task<(Comment Comment, Error Error)>>();
            foreach (var each in _comments.State ?? Enumerable.Empty<long>())
            {
                var commentGrain = _factory.GetGrain<ICommentGrain>(each, slug);
                tasks.Add(commentGrain.Get(currentUser));
            }
            var result = (await Task.WhenAll(tasks)).Select(x => x.Comment).ToList();
            return (result, Error.None);
        }
    }
}
