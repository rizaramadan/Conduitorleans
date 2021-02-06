using Xunit;
using Conduit.Features.Articles;
using System;
using System.Collections.Generic;
using System.Text;
using Contracts.Users;
using Moq;
using Orleans;
using System.Threading.Tasks;
using System.Threading;
using Contracts;
using Conduit.Features.Articles.Outputs;
using Contracts.Articles;

namespace Conduit.Features.Articles.Tests
{
    class TestArticleGrain : IArticlesGrain
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string User { get; set; }

        public async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)> 
            GetHomeGuestArticles(string currentUser, int limit, int offset)
        {
            Limit  = limit;
            Offset = offset;
            User   = currentUser;
            return await Task.FromResult(
            (
                new List<ArticleUserPair>(),
                ulong.Parse("0"),
                Error.None
            ));
        }
    }


    public class GetArticlesHandlerTests
    {
        [Fact()]
        public async Task GetArticlesHandlerTest()
        {
            var grain = new TestArticleGrain();
            var client = new Mock<IClusterClient>();
            client.Setup(x => x.GetGrain<IArticlesGrain>(0, It.IsAny<string>())).Returns(grain);
            var userSvcMock = new Mock<IUserService>();
            userSvcMock.Setup(x => x.GetCurrentUsername()).Returns("auser");
            var articleSvc = new ArticleService();
            var getArticleHandler = 
                new GetArticlesHandler(client.Object, userSvcMock.Object, articleSvc);

            (GetArticlesOutput Output, Error Error) = 
                await getArticleHandler.Handle(new GetArticlesInput(), CancellationToken.None);

            Assert.Equal(Error.None, Error);
            Assert.Equal(ulong.Parse("0"),Output.ArticlesCount);
            Assert.Equal(10, grain.Limit);
            Assert.Equal(0, grain.Offset);
            Assert.Equal("auser", grain.User);
        }
    }
}