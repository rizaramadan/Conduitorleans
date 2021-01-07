namespace Grains.Articles
{
    using Contracts;
    using Contracts.Articles;
    using Contracts.Tags;
    using Contracts.Users;
    using Orleans;
    using Orleans.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ArticleGrain : Grain, IArticleGrain
    {
        private static Guid ErrorGetGuid = Guid.Parse("1abf835e-6e1f-45b1-b6cd-7ddef724673e");
        
        private readonly IPersistentState<Article> _article;
        private readonly IGrainFactory _factory;

        public ArticleGrain(
            [PersistentState("UserGrain", Constants.GrainStorage)] IPersistentState<Article> s,
            IGrainFactory f
        )
        {
            _article = s;
            _factory = f;
        }

        public async Task<Error> CreateArticle(Article article)
        {
            var articleId = this.GetPrimaryKeyLong(out var username);
            await AddArticleToTags(article, articleId, username);
            await SaveArticle(article, username);
            return Error.None;
        }

        private async Task SaveArticle(Article article, string username)
        {
            _article.State.Title = article.Title;
            _article.State.Slug = article.Slug;
            _article.State.Body = article.Body;
            _article.State.CreatedAt = DateTime.Now;
            _article.State.UpdatedAt = _article.State.CreatedAt;
            _article.State.Description = article.Description;
            _article.State.TagList = article.TagList;
            _article.State.Author = username;
            _article.State.Favorited = new List<User>(0);
            _article.State.FavoritesCount = 0;
            await _article.WriteStateAsync();
        }

        private async Task AddArticleToTags(Article article, long articleId, string username)
        {
            var taskList = new List<Task<Error>>();
            foreach (var tag in article.TagList ?? Enumerable.Empty<string>())
            {
                var tagGrain = _factory.GetGrain<ITagGrain>(tag);
                var task = tagGrain.AddArticle(articleId, username);
                taskList.Add(task);
            }
            await Task.WhenAll(taskList);
        }

        public async Task<(Article Article, Error Error)> GetArticle()
        {
            try
            {
                return (_article.State, Error.None);
            }
            catch (Exception ex) 
            {
                return await Task.FromResult<(Article, Error)>((null,new Error(ErrorGetGuid, ex.Message)));
            }
        }
    }
}
