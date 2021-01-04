
namespace Conduit.Features.Articles.Inputs
{
    public class GetArticlesInput
    {
        public string Tag { get; set; }
        public string Autor { get; set; }
        public string Favorited { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
    }
}
