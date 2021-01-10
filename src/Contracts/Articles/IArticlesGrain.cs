namespace Contracts.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts.Users;
    using Orleans;


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

    public interface IArticlesGrain : IGrainWithIntegerKey
    {
        public Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)> GetHomeGuestArticles(int limit, int offset);
    }
}
