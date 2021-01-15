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
            [PersistentState(nameof(ArticleGrain), Constants.GrainStorage)] IPersistentState<Article> s,
            IGrainFactory f
        )
        {
            _article = s;
            _factory = f;
        }

        public async Task<Error> CreateArticle(Article article)
        {
            try
            {
                var articleId = this.GetPrimaryKeyLong(out var username);
                await AddArticleToTags(article, articleId, username);
                await SaveArticle(article, username);
                return Error.None;
            }
            catch (Exception ex) 
            {
                return new Error("e3ee566e-391b-42d8-ac66-2e90aa8bf71b", ex.Message);
            }
        }

        private async Task SaveArticle(Article article, string username)
        {
            _article.State.Title = article.Title;
            _article.State.Slug = article.Title.GenerateSlug();
            _article.State.Body = article.Body;
            _article.State.CreatedAt = DateTime.Now;
            _article.State.UpdatedAt = _article.State.CreatedAt;
            _article.State.Description = article.Description;
            _article.State.TagList = article.TagList;
            _article.State.Author = new Profile { Username = username };
            _article.State.Favorited = false;
            await _article.WriteStateAsync();
            var counter = _factory.GetGrain<ICounterGrain>(nameof(IArticleGrain));
            var countTask = counter.Increement();
            var userArticles = _factory.GetGrain<IUserArticlesGrain>(username);
            var userTask = userArticles.AddArticle(this.GetPrimaryKeyLong(out var ext));
            await Task.WhenAll(countTask, userTask);
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

        public async Task<(Article Article, Error Error)> Get()
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

        public async Task<Error> UpdateArticle(string title, string body, string description)
        {
            if (!string.IsNullOrEmpty(title))
            {
                _article.State.Title = title;
                _article.State.Slug = title.GenerateSlug();
            }
            if (!string.IsNullOrEmpty(body))
            {
                _article.State.Body = body;
            }
            if (!string.IsNullOrEmpty(description))
            {
                _article.State.Description = description;
            }
            await _article.WriteStateAsync();
            return Error.None;
        }

        public async Task<Error> AddFavorited(string user)
        {
            if (_article.State.Favorites == null) 
            {
                _article.State.Favorites = new List<string>(1);
            }
            _article.State.Favorites.Add(user);
            await _article.WriteStateAsync();
            return Error.None;
        }

        public async Task<Error> RemoveFavorited(string user)
        {
            _article.State.Favorites.RemoveAll(x => x.Equals(user));
            await _article.WriteStateAsync();
            return Error.None;
        }
    }
}
