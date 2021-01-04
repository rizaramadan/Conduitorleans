using Contracts.Articles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Features.Articles.Outputs
{

    public class ArticlesOutput
    {
        public List<IArticle> Articles { get; set; }
        public int ArticleCount { get; set; }
    }
}
