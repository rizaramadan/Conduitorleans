namespace Contracts.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Orleans;

    public interface IFeedGrain : IGrainWithStringKey
    {
        Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            Get(string currentUser, int limit, int offset);
    }
}
