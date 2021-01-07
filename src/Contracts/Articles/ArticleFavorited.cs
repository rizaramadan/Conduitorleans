
using Contracts.Users;

namespace Contracts.Articles
{
    public class ArticleFavorited
    {
        User User { get; set; }
        Article Article { get; set; }
    }
}

