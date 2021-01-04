
using Contracts.Users;

namespace Contracts.Articles
{
    public interface IArticleFavorited
    {
        IUser User { get; set; }
        IArticle Article { get; set; }
    }
}

