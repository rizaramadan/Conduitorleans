namespace Grains.Comments
{
    using Contracts;
    using Contracts.Comments;
    using Contracts.Follows;
    using Contracts.Users;
    using Orleans;
    using Orleans.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using PersistenceState = Orleans.Runtime.IPersistentState<Contracts.Comments.Comment>;

    public class CommentGrain : Grain, ICommentGrain
    {
        private readonly PersistenceState _comment;
        private readonly IGrainFactory _factory;

        public CommentGrain(
            [PersistentState(nameof(CommentGrain), Constants.GrainStorage)] PersistenceState s,
            IGrainFactory f
        )
        {
            _comment = s;
            _factory = f;
        }

        public async Task<(Comment Comment, Error Error)> Get(string username)
        {
            var userGrain = _factory.GetGrain<IUserGrain>(_comment.State.Author.Username);
            (User User, Error Error) = await userGrain.Get();
            if (Error.Exist()) 
            {
                return (null, Error);
            }

            var result = _comment.State;
            var following = _factory.GetGrain<IUserFollowingGrain>(username);
            result.Author = new Profile
            {
                Username = User.Username,
                Bio = User.Bio,
                Following = await following.IsFollow(User.Username),
                Image = User.Image
            };
            return (result, Error.None);
        }

        public async Task<Error> Set(Comment comment)
        {
            _comment.State = comment;
            await _comment.WriteStateAsync();
            return Error.None;
        }
    }
}
