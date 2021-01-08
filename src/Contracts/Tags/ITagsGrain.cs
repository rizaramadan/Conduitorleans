namespace Contracts.Tags
{
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface ITagsGrain : IGrainWithIntegerKey
    {
        Task<(List<string> Tags, Error Error)> GetTags();
    }
}
