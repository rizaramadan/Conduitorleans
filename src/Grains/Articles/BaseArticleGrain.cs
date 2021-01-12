namespace Grains.Articles
{
    using Contracts;
    using Contracts.Articles;
    using Contracts.Users;
    using Orleans;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class BaseArticleGrain : Grain
    {
        protected readonly IGrainFactory _factory;
        public BaseArticleGrain(IGrainFactory f)
        {
            _factory = f;
        }

        protected async Task<List<ArticleUserPair>> 
            GetArticlesData(List<(long ArticleId, string Author)> list)
        {

            var articleTasks = new List<Task<(Article article, Error error)>>(list.Count);
            var authorTasks = new List<Task<(User User, Error Error)>>(list.Count);
            var authorSet = new HashSet<string>();
            foreach (var each in list)
            {
                var articleGrain = _factory.GetGrain<IArticleGrain>(each.ArticleId, each.Author);
                articleTasks.Add(articleGrain.GetArticle());
                if (!authorSet.Contains(each.Author))
                {
                    authorSet.Add(each.Author);
                    var userGrain = _factory.GetGrain<IUserGrain>(each.Author);
                    authorTasks.Add(userGrain.Get());
                }
            }

            var articles = (await Task.WhenAll(articleTasks)).ToList();
            var authors = (await Task.WhenAll(authorTasks)).ToDictionary(x => x.User.Username, y => y.User);

            return articles.Where(x => !x.error.Exist())
                .Select(x => new ArticleUserPair(x.article, authors[x.article.Author]))
                .ToList();
        }
    }
}
