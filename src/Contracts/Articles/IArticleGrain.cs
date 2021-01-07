using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Contracts.Articles
{
    public interface IArticleGrain : IGrainWithIntegerCompoundKey
    {
        //TODO: create grain implementation
        Task<Error> CreateArticle(Article article);
        Task<(Article Article, Error Error)> GetArticle();
    }
}
