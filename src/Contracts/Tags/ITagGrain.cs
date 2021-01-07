using Contracts.Articles;
using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts.Tags
{
    public interface ITagGrain : IGrainWithStringKey
    {
        Task<(List<(long ArticleId, string Author)>, Error)> GetArticles();
        Task<Error> AddArticle(long articleId, string author);
        Task<Error> RemoveArticle(long articleId, string author);
    }
}
