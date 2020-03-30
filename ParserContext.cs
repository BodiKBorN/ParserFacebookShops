using System.Dynamic;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Io;
using PuppeteerSharp;

namespace ParserFacebookShops
{
    internal static class ParserContext
    {
        //static ParserContext()
        //{
        //    Task.Run(GetBrowserAsync);
        //}

        public static IBrowsingContext AngleSharpContext { get;}  
            = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithDefaultCookies());

        public static Browser PuppeteerSharpContext { get; } 
            = Task.Run(GetBrowserAsync).Result;


        private static async Task<Browser> GetBrowserAsync()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            //Authentication 
            var page = await browser.NewPageAsync();

            await page.GoToAsync("https://www.facebook.com/");
            await page.EvaluateExpressionAsync("document.querySelector('#email').value = 'bodik_kz@ukr.net'");
            await page.EvaluateExpressionAsync("document.querySelector('#pass').value = '201001chepa'");
            await page.EvaluateExpressionAsync("document.querySelector('#loginbutton').click()");
            await page.WaitForNavigationAsync();

            return browser;
        }
    }
}