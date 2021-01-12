namespace Contracts.Articles
{
    using Contracts.Users;

    public class ArticleUserPair
    {
        public Article Article { get; }
        public User User { get; }
        public ArticleUserPair(Article a, User u)
        {
            Article = a;
            User = u;
        }
    }
}
