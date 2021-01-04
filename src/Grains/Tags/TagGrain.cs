﻿namespace Grains.Tags
{
    using Contracts;
    using Contracts.Tags;
    using Orleans;
    using Orleans.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class TagGrain : Grain, ITagGrain
    {
        private readonly IPersistentState<TagState> _tagState;
        private readonly IGrainFactory _factory;

        public TagGrain(
            [PersistentState(nameof(TagGrain), Constants.GrainStorage)] IPersistentState<TagState> s,
            IGrainFactory f
        )
        {
            _tagState = s;
            _factory = f;
            if (_tagState.State.ArticleIds == null)
            {
                _tagState.State.ArticleIds = new HashSet<long>();
            }
        }

        public async Task<IError> AddArticle(long articleId)
        {
            try
            {
                _tagState.State.ArticleIds.Add(articleId);
                return Error.None;
            }
            catch (Exception ex)
            {
                return await Task.FromResult(
                       new Error("1b3b5a91-4f13-472f-b878-11f4cf103c36", ex.ToString())
                );
            }
        }

        public async Task<(List<long>, IError)> GetArticles()
        {
            var list = _tagState.State.ArticleIds.ToList();
            return await Task.FromResult((list, Error.None));
        }

        public async Task<IError> RemoveArticle(long articleId)
        {
            try
            {
                _tagState.State.ArticleIds.Remove(articleId);
                return Error.None;
            }
            catch (Exception ex)
            {
                return await Task.FromResult(
                       new Error("1f04e560-0d38-4330-abae-59475f578a3e", ex.ToString())
                );
            }
        }
    }
}
