using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using PuppeteerSharp;
using Request = AngleSharp.Io.Request;

namespace ParserFacebookShops
{
    class Program
    {
        private const string MagicSpace = " ";
        static async Task Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await Parse("https://www.facebook.com/pg/nusho.shop/");
            stopwatch.Stop();
            var stopwatchElapsed = stopwatch.Elapsed;

            Console.WriteLine("\tProducts\t\n ---------------------------\n ");

            foreach (var variable in result)
            {
                Console.WriteLine($"Name: {variable.Name}\nPrice: {variable.Price.Cost}{variable.Price.Currency}\nTotal: {variable.Price.Total }\nDescription: {variable.Description}\nPast Price: {variable.Price.Cost}{variable.PastPrice.Currency}\n");
            }
        }

        public static async Task<List<ProductModel>> Parse(string shopUrl)
        {
            try
            {

                //Implementation authentication with AngleSharp
                //WebContext.Context.OpenAsync("https://www.facebook.com").Wait();
                //(WebContext.Context.Active.QuerySelector<IHtmlInputElement>("input#email")).Value = "bodik_kz@ukr.net";
                //(WebContext.Context.Active.QuerySelector<IHtmlInputElement>("input#pass")).Value = "201001chepa";
                //await (WebContext.Context.Active.QuerySelector("#loginbutton").Children.FirstOrDefault() as IHtmlInputElement)
                //    .SubmitAsync();

                if (!shopUrl.EndsWith("/"))
                    shopUrl += "/";

                using var document = await WebContext.Context.OpenAsync(shopUrl + "shop/");

                await document.WaitForReadyAsync();
                IHtmlCollection<IElement> querySelector = null;
                var selectorResult = document.QuerySelectorAll("#content_container table > tbody > tr td");

                querySelector = selectorResult.Length != 0 
                    ? selectorResult 
                    : (await GetAllProductPage()).Data;

                List<ProductModel> productModels = null;

                if (querySelector.Length != 0)
                    productModels = querySelector
                        .Where(x => x.QuerySelector("div > div > div > a > strong")?.InnerHtml != null)
                        .Select(x =>
                        {
                            var product = new ProductModel
                            {
                                Name = x.QuerySelector("div > div > div > a > strong")?.InnerHtml,
                            };

                            var price = GetPrice(x.QuerySelector("div > div > div > div span")?.InnerHtml ??
                                                 x.QuerySelector("div > div > div > div")?.InnerHtml);

                            if (price.Success)
                                product.Price = price.Data;

                            var pastPrice = GetPrice(x.QuerySelector("div > div > div > div span:nth-child(2)")?.InnerHtml);

                            if (pastPrice.Success)
                                product.PastPrice = pastPrice.Data;

                            var htmlAnchorElement = (IHtmlAnchorElement) x.QuerySelector("div > div > div > a");

                            var productCard = GetProductCard(htmlAnchorElement.Href);

                            var image = x.QuerySelector("div > div > a img");

                            if (image != null)
                                product.Image = ((IHtmlImageElement) image).Source;

                            return product;
                        }).ToList();

                document.Close();
                //document.Dispose();

                return productModels;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static IResult<Price> GetPrice(string htmlPrice)
        {
            try
            {
                if (htmlPrice == null)
                    return Result<Price>.CreateFailed("NOT_CORRECT_DATA");

                var htmlDecode = HttpUtility.HtmlDecode(htmlPrice);

                var price = new Price();

                if (htmlDecode.Contains(MagicSpace))
                    price.Total = htmlDecode.Replace(MagicSpace, " ");

                var cost = GetCost(htmlDecode);

                if (cost.Success)
                    price.Cost = cost.Data;

                var currency = GetCurrency(htmlDecode);

                if (currency.Success)
                    price.Currency = currency.Data;

                return Result<Price>.CreateSuccess(price);
            }
            catch
            {
                return Result<Price>.CreateFailed("ERROR_GET_PRICE");
            }
        }

        public static IResult<double> GetCost(string htmlDecode)
        {
            try
            {
                var regex = new Regex(@"(\d+(\,|\.)?\d+)|(\d)");

                var value = regex.Match(htmlDecode.Replace(MagicSpace, string.Empty)).Value;

                var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                
                numberFormat.NumberDecimalSeparator = new Regex(@"(\,|\.)").Match(value).Value;

                return double.TryParse(value, NumberStyles.Currency, numberFormat, out var cost)
                    ? Result<double>.CreateSuccess(cost)
                    : Result<double>.CreateFailed("COST_NOT_PARSE");
            }
            catch
            {
                return Result<double>.CreateFailed("ERROR_GET_COST");
            }
        }

        public static IResult<string> GetCurrency(string htmlDecode)
        {
            try
            {
                var whiteSpaces = new[] { ' ', ' ' };

                if (htmlDecode == null || !htmlDecode.Any(x => whiteSpaces.Contains(x)))
                    Result<string>.CreateFailed("NOT_CORRECT_DATA");

                return Result<string>.CreateSuccess(htmlDecode?.Substring(htmlDecode.LastIndexOfAny(whiteSpaces)).Trim());
            }
            catch
            {
                return Result<string>.CreateFailed("ERROR_GET_CURRENCY");
            }
        }

        public static async Task<IResult<string>> GetProductCard(string cardUrl)
        {
            try
            {
                if (cardUrl == null)
                    Result<IElement>.CreateFailed();

                var openAsync = await WebContext.Context.OpenAsync(cardUrl);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<IResult<IHtmlCollection<IElement>>> GetAllProductPage()
        {
            try
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });

                var page = await browser.NewPageAsync();

                await page.GoToAsync("https://www.facebook.com/pg/nusho.shop/shop/");

                //Authentication 
                await page.EvaluateExpressionAsync("document.querySelector('#email').value = 'bodik_kz@ukr.net'");
                await page.EvaluateExpressionAsync("document.querySelector('#pass').value = '201001chepa'");
                await page.EvaluateExpressionAsync("document.querySelector('#loginbutton').click()");
                await page.WaitForNavigationAsync();

                await page.EvaluateExpressionAsync("scrollTo(0, document.querySelector('body').scrollHeight)");

                var waitForSelectorResult = await page.WaitForSelectorAsync("#u_0_4 > ul > li:nth-child(5) > div > div.clearfix._2pi9._2pin._2pic > div.rfloat._ohf > a");

                var href = await (await waitForSelectorResult.GetPropertyAsync("href")).JsonValueAsync<string>()
                           ?? (await page.EvaluateExpressionAsync("(function() {let node = document.querySelector('#u_0_4 > ul > li:nth-child(5) > div > div.clearfix._2pi9._2pin._2pic > div.rfloat._ohf > a'); return !!node ? node.href : null})()")).ToString(); ;

                var s1 = (await waitForSelectorResult.GetPropertyAsync("href")).ToString();
                var s = (await page.EvaluateExpressionAsync("(function() {let node = document.querySelector('#u_0_4 > ul > li:nth-child(5) > div > div.clearfix._2pi9._2pin._2pic > div.rfloat._ohf > a'); return !!node ? node.href : null})()")).ToString();
                await browser.CloseAsync();

                using var document = await WebContext.Context.OpenAsync(href);

                var result = document.QuerySelectorAll("tbody > tr td");

                document.Close();

                return result == null 
                    ? Result<IHtmlCollection<IElement>>.CreateFailed("ELEMENTS_NOT_FOUND") 
                    : Result<IHtmlCollection<IElement>>.CreateSuccess(result);
            }
            catch (Exception e)
            {
                return Result<IHtmlCollection<IElement>>.CreateFailed("ERROR_GET_ALL_PRODUCT_PAGE");
            }
        }
    }
}
