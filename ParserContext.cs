using System.Dynamic;
using AngleSharp;
using AngleSharp.Io;

namespace ParserFacebookShops
{
    internal static class ParserContext
    {
        public static IBrowsingContext AngleSharpContext { get;}  = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithDefaultCookies().WithJs());
    }
}