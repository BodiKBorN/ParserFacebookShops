using System.Dynamic;
using AngleSharp;
using AngleSharp.Io;

namespace ParserFacebookShops
{
    public static class WebContext
    {
        public static IBrowsingContext Context { get;}  = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithDefaultCookies().WithJs());
    }
}