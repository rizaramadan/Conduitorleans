using Conduit.Features.Articles.Outputs;

namespace Conduit.Features.Articles
{
    public enum ArticlesListType
    {
        All,
        Feed,
        Favorited,
        Authored,
        ByTag
    }

    public interface IArticleService 
    {
        ArticlesListType GetListType(GetArticlesInput input);
    }

    public class ArticleService : IArticleService
    {
        public ArticlesListType GetListType(GetArticlesInput i)
        {
            if (!string.IsNullOrWhiteSpace(i.Tag))
            {
                return ArticlesListType.ByTag;
            }
            
            if (i.Feed)
            {
                return ArticlesListType.Feed;
            }

            if (!string.IsNullOrWhiteSpace(i.Author))
            {
                return ArticlesListType.Authored;
            }

            if (!string.IsNullOrWhiteSpace(i.Favorited)) 
            {
                return ArticlesListType.Favorited;
            }

            return ArticlesListType.All;
        }
    }
}
