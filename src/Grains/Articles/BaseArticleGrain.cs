namespace Grains.Articles
{
    using Contracts;
    using Contracts.Articles;
    using Contracts.Follows;
    using Contracts.Users;
    using Orleans;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class BaseArticleGrain : Grain
    {
        protected readonly IGrainFactory _factory;
        public BaseArticleGrain(IGrainFactory f) => _factory = f;

        protected async Task<List<ArticleUserPair>> 
            GetArticlesData(string currentUser, List<(long ArticleId, string Author)> list)
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
            var authors = (await Task.WhenAll(authorTasks))
                .ToDictionary(x => x.User.Username, y =>new Profile
                { 
                    Username  = y.User.Username,
                    Bio       = y.User.Bio,
                    Image     = y.User.Image,
                });

            if (!string.IsNullOrWhiteSpace(currentUser))
            {
                await AddFollowingInfo(currentUser, authors);
            }

            return articles.Where(x => !x.error.Exist())
                .Select(x => new ArticleUserPair(x.article, authors[x.article.Author]))
                .ToList();
        }

        private async Task AddFollowingInfo(string currentUser, Dictionary<string, Profile> authors)
        {
            var articleAuthors = authors.Keys.OrderBy(x => x).ToList();
            var followingTask = new List<Task<bool>>(articleAuthors.Count);
            foreach (var each in articleAuthors)
            {
                var followingGrain = _factory.GetGrain<IUserFollowingGrain>(currentUser);
                followingTask.Add(followingGrain.IsFollow(each));
            }
            var result = await Task.WhenAll(followingTask);
            var followMap = new Dictionary<string, bool>();
            for (var i = 0; i < articleAuthors.Count; i++)
            {
                followMap[articleAuthors[i]] = result[i];
            }

            var followsAuhtor = followMap.Keys.ToList();
            foreach (var each in articleAuthors)
            {
                authors[each].Following = followMap[each];
            }
        }
    }
}
