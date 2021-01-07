namespace Grains.Articles
{
    using Contracts;
    using Contracts.Articles;
    using Contracts.Users;
    using Orleans;
    using Orleans.Runtime;
    using System;
    using System.Collections.Generic;
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
            this.GetPrimaryKeyLong(out var username);
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
            return Error.None;
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
