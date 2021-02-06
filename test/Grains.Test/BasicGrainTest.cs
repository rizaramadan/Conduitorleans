using Contracts;
using Contracts.Users;
using Grains.Users;
using Moq;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Grains.Test
{
    public class BasicGrainTest
    {
        [Fact]
        public async Task SiloHasRegisteredTest()   
        {

            var persistenceState = new Mock<IPersistentState<User>>();
            var user = new User 
            {
                Username = string.Empty,
                Email    = string.Empty,
                Password = new byte[0],
                Bio      = string.Empty,
                Salt     = Guid.Empty,
                Image    = string.Empty

            };
            persistenceState.Setup(x => x.State).Returns(user);
            var userGrain = new UserGrain(persistenceState.Object, null);

            Assert.NotNull(userGrain);

            (bool, Error) result = await userGrain.HasRegistered();

            Assert.False(result.Item1); //unsuccessful has registered check will return false
            Assert.Equal(Error.None,result.Item2); //error should be none
        }
    }
}
