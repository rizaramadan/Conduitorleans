using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts.Tags
{
    public interface ITagGrain : IGrainWithStringKey
    {
        Task<(List<long>, IError)> GetArticles();
        Task<IError> AddArticle(long articleId);
        Task<IError> RemoveArticle(long articleId);
    }
}
