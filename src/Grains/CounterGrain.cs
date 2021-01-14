namespace Grains
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Orleans;
    using Orleans.Runtime;

    public class CounterState
    {
        public ulong Current { get; set; }
        public CounterState() => Current = 0;
    }

    public class CounterGrain : Grain, ICounterGrain
    {
        private readonly IPersistentState<CounterState> _counter;

        public CounterGrain(
            [PersistentState("UserGrain", Constants.GrainStorage)] IPersistentState<CounterState> c
        )
        {
            _counter = c;
        }
        public async Task<Error> Decreement()
        {
            try
            {
                if (_counter.State.Current > 0)
                {
                    _counter.State.Current--;
                    await _counter.WriteStateAsync();
                    return Error.None;
                }
                return new Error("3cde47be-ef2a-4b32-ad49-23857fb660a2", $"{this.GetPrimaryKeyString()} is zero");
            }
            catch (Exception ex)
            {
                return new Error("d5337b6b-87af-4b3a-aed8-b6af42f94eb5", $"{this.GetPrimaryKeyString()}{ex.Message}");
            }
        }

        public async Task<ulong> Get()
        {
            return await Task.FromResult(_counter.State.Current);
        }

        public async Task<Error> Increement()
        {
            try
            {
                _counter.State.Current++;
                await _counter.WriteStateAsync();
                return Error.None;
            }
            catch (Exception ex)
            {
                return new Error("9387110b-0dfb-4797-b4ce-a8153a7fe99a", $"{this.GetPrimaryKeyString()}{ex.Message}");
            }
        }
    }
}
