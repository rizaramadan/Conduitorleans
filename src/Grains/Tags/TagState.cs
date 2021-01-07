namespace Grains.Tags
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    public class TagState
    {
        public HashSet<string> ArticleIds { get; set; }
        public TagState() => ArticleIds = new HashSet<string>();
    }
}
