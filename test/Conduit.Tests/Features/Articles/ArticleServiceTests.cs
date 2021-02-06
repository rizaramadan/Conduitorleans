using Xunit;
using Conduit.Features.Articles;

namespace Conduit.Features.Articles.Tests
{
    public class ArticleServiceTests
    {
        [Fact()]
        public void GetListTypeTest()
        {
            var svc = new ArticleService();
            var output = svc.GetListType(new GetArticlesInput { Tag = "sometag" });
            Assert.Equal(ArticlesListType.ByTag, output);

            output = svc.GetListType(new GetArticlesInput { Feed = true });
            Assert.Equal(ArticlesListType.Feed, output);

            output = svc.GetListType(new GetArticlesInput { Author = "some author" });
            Assert.Equal(ArticlesListType.Authored, output);

            output = svc.GetListType(new GetArticlesInput { Favorited = "favorited" });
            Assert.Equal(ArticlesListType.Favorited, output);
        }
    }
}