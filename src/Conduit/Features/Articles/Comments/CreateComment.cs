using Contracts;
using Contracts.Comments;
using Contracts.Users;
using FluentValidation;
using MediatR;
using Orleans;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Features.Articles.Comments
{
    public class CommentWrapper
    {
        public Comment Comment {get;set;}
    }

    public class CreateCommentValidator : AbstractValidator<CommentWrapper>
    {
        public CreateCommentValidator()
        {
            RuleFor(c => c.Comment).NotNull();
            RuleFor(c => c.Comment.Body).NotEmpty();
        }
    }

    public class CreateComment : IRequest<(Comment Comment, Error Error)>
    {
        public string Slug { get; set; }
        public Comment Comment { get; set; }

        public CreateComment(string slug, Comment c)
        {
            this.Slug = slug;
            this.Comment = c;
        }
    }



    public class CreateCommentHandler : IRequestHandler<CreateComment, (Comment Comment, Error Error)>
    {
        private readonly IClusterClient _client;
        private readonly IUserService _userService;

        public CreateCommentHandler(IMediator m, IClusterClient c, IUserService u)
        {
            _client = c;
            _userService = u;
        }

        public async Task<(Comment Comment, Error Error)> Handle(CreateComment req, CancellationToken ct)
        {
            var comment = req.Comment;
            var commentsGrain = _client.GetGrain<ICommentsGrain>(req.Slug);
            (long Id, Error Error) = await commentsGrain.AddComment(_userService.GetCurrentUsername(), req.Slug, comment);
            if (Error.Exist()) 
            {
                return (null, Error);
            }
            var commentGrain = _client.GetGrain<ICommentGrain>(Id, req.Slug);
            return await commentGrain.Get(_userService.GetCurrentUsername());
        }
    }
}