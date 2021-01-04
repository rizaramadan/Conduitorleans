namespace Grains.Tags
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    public class TagState
    {
        public HashSet<long> ArticleIds { get; set; }
    }
}
