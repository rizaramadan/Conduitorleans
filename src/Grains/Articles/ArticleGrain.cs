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
        private readonly IPersistentState<ArticleState> _article;
        private readonly IGrainFactory _factory;

        public ArticleGrain(
            [PersistentState("UserGrain", Constants.GrainStorage)] IPersistentState<ArticleState> s,
            IGrainFactory f
        )
        {
            _article = s;
            _factory = f;
        }

        public async Task<IError> CreateArticle(IArticle article)
        {
            _article.State.Title = article.Title;
            _article.State.Slug = article.Slug;
            _article.State.Body = article.Body;
            _article.State.CreatedAt = DateTime.Now;
            _article.State.UpdatedAt = _article.State.CreatedAt;
            _article.State.Description = article.Description;
            _article.State.TagList = article.TagList;
            _article.State.Author = article.Author;
            _article.State.Favorited = new List<IUser>(0);
            _article.State.FavoritesCount = 0;
            await _article.WriteStateAsync();
            return Error.None;
        }
    }
}
