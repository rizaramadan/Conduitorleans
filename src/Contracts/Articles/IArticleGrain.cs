using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Contracts.Articles
{
    public interface IArticleGrain : IGrainWithIntegerKey
    {
    }
}
