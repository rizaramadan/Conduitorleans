namespace Contracts.Articles
{
    using Contracts.Users;

    public class ArticleUserPair
    {
        public Article Article { get; }
        public Profile User { get; }
        public ArticleUserPair(Article a, Profile u)
        {
            Article = a;
            User = u;
        }
    }
}
