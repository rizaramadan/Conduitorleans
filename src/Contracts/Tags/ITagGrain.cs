using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts.Tags
{
    public interface ITagGrain : IGrainWithStringKey
    {
        Task<(List<long>, Error)> GetArticles();
        Task<Error> AddArticle(long articleId);
        Task<Error> RemoveArticle(long articleId);
    }
}
