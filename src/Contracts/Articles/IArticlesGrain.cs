namespace Contracts.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Orleans;

    public interface IArticlesGrain : IGrainWithIntegerKey
    {
        public Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetHomeGuestArticles(string currentUser, int limit, int offset);

        
    }
}
