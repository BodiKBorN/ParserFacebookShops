using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using PuppeteerSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ParserFacebookShops.Services.Implementation
{
    internal class PuppeteerSharpParser : IDisposable
    {
        public Browser Context { get; }

        private readonly IProductService _productService;

        public PuppeteerSharpParser()
        {
            Context = GetBrowserAsync().Result;
            _productService = new ProductService();
        }

        public async Task<Page> OpenPageAsync(string address)
        {
            try
            {
                var page = Context.NewPageAsync().Result;

                await page.GoToAsync(address);

                return page;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<IResult<string>> GetHrefAllProductsPageAsync(string address)
        {
            try
            {
                using var page = await OpenPageAsync(address);
               
                await page.EvaluateExpressionAsync("scrollTo(0, document.querySelector('body').scrollHeight)");

                await page.WaitForTimeoutAsync(1000);

                var selectorResult = await page.QuerySelectorAsync("div._4-u8 ul:last-child > li:last-child > div > div > div > a");
                
                if (selectorResult == null)
                {
                    for (var i = 0; i < 10; i++)
                    {
                        await page.EvaluateExpressionAsync("scrollTo(0, document.querySelector('body').scrollHeight)");
                        await page.WaitForTimeoutAsync(2000);
                        selectorResult = await page.QuerySelectorAsync("div._4-u8 ul:last-child > li:last-child > div > div > div > a");
                        if (selectorResult != null)
                        {
                            break;
                        }
                    }
                }

                if (selectorResult == null)
                    return Result<string>.CreateFailed("SELECT_ELEMENTS_ERROR");

                var href = await (await selectorResult.GetPropertyAsync("href")).JsonValueAsync<string>()
                           ?? await page.EvaluateExpressionAsync<string>(
                               "(function() {let node = document.querySelector('div._4-u8 ul:last-child > li:last-child > div > div > div > a'); return !!node ? node.href : null})()");

                await page.CloseAsync();

                return href == null
                    ? Result<string>.CreateFailed("PAGE_HREF_NOT_FOUND")
                    : Result<string>.CreateSuccess(href);
            }
            catch
            {
                return Result<string>.CreateFailed("ERROR_GET_ALL_PRODUCT_PAGE");
            }
        }

        public async Task<IResult<Product>> GetProductCard(string cardUrl)
        {
            try
            {
                using var page = await OpenPageAsync(cardUrl);

                var pageLanguage = await page
                    .EvaluateExpressionAsync<string>(
                        "(function() {let node = document.querySelector('html'); return !!node ? node.getAttribute('lang') : null})()");
                
                var name = await page
                    .EvaluateExpressionAsync<string>(
                        "(function() {let node = document.querySelector('#u_0_y > div'); return !!node ? node.innerText : null})()");

                var htmlPrice = await page
                    .EvaluateExpressionAsync<string>(
                        "(function() {let node = document.querySelector('#u_0_y > div > div > div > div > div > div'); return !!node ? node.innerText : null})()");

                var category = await page
                    .EvaluateExpressionAsync<string>(
                        "(function() {let node = document.querySelector('#u_0_y > div > div > span > div > div > div > a > span'); return !!node ? node.innerText : null})()");

                var showMoreButton = await page
                    .QuerySelectorAsync("#u_0_y > ul > li > div > div");

                if (showMoreButton != null)
                    await showMoreButton.ClickAsync();

                var description = await page
                    .EvaluateExpressionAsync<string>(
                        "(function() {let node = document.querySelector('#u_0_y > ul > li > div._1xwp'); return !!node ? node.innerText : null})()");

                var imagesUrl = (await page.EvaluateExpressionAsync(
                        "Array.from(document.querySelectorAll('div > div > div > div > div > div > div > div > div > div > div > div._6e_ > div')).map(x=>x.style.backgroundImage)"))
                    .Values<string>()
                    .Select(htmlImageUrl => _productService.ParseImageUrl(htmlImageUrl).Data)
                    .ToList();

                var price = _productService.GetPrice(htmlPrice,pageLanguage);

                if (name == null && htmlPrice == null)
                    return Result<Product>.CreateFailed("ITEM_DATA_NOT_FOUND");

                if (!price.Success)
                    return Result<Product>.CreateFailed(price.Message);

                var product = new Product()
                {
                    Name = name,
                    Price = price.Data,
                    Description = description,
                    Category = category,
                    ImagesUrl = imagesUrl
                };

                await page.CloseAsync();

                return Result<Product>.CreateSuccess(product);
            }
            catch
            {
                return Result<Product>.CreateFailed("GETTING_PRODUCT_CARD_ERROR");
            }
        }

        private async Task<Browser> GetBrowserAsync()
        {
            try
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });

                //Authentication for Facebook
                var page = await browser.NewPageAsync();

                await page.GoToAsync("https://www.facebook.com/");
                await page.EvaluateExpressionAsync($"document.querySelector('#email').value = '{AuthenticationData.Login}'");
                await page.EvaluateExpressionAsync($"document.querySelector('#pass').value = '{AuthenticationData.Password}'");
                await page.EvaluateExpressionAsync("document.querySelector('#loginbutton').click()");
                await page.WaitForNavigationAsync();

                return browser;
            }
            catch
            {
                Console.WriteLine("GETTING_BROWSER_ERROR");
                throw;
            }
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}