using System.Dynamic;
using AngleSharp;

namespace ParserFacebookShops
{
    public static class WebContext
    {
        public static IBrowsingContext Context { get;}  = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    }
}