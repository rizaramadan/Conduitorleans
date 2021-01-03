using Contracts;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Grains.Hello
{
    public class HelloGrain : Grain, IHello
    {
        private readonly IPersistentState<HelloState> _helloState;

        public HelloGrain([PersistentState("hello", Constants.GrainStorage)] IPersistentState<HelloState> s)
        {
            _helloState = s;
        }
        public async Task<string> SayHello(string greeting)
        {
            _helloState.State.Count++;
            await _helloState.WriteStateAsync();
            return $"\n Client said: '{greeting}', so HelloGrain says: Hello! {greeting}, for the {_helloState.State.Count} time(s)";
        }
    }
}
