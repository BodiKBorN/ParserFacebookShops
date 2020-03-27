using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using ParserFacebookShops.Services.Implementation;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ParserFacebookShops
{
    class Program
    {
        private const string MagicSpace = " ";

        static async Task Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //var result = await Parse("https://www.facebook.com/pg/nusho.shop/");
            var shopService = new ShopService();

            var shop = await shopService.GetProductsAsync("https://www.facebook.com/pg/nusho.shop/");

            stopwatch.Stop();
            var stopwatchElapsed = stopwatch.Elapsed;

            Console.WriteLine("\tProducts\t\n ---------------------------\n ");

            //foreach (var variable in result)
            //{
            //    Console.WriteLine($"Name: {variable.Name}\nPrice: {variable.Price.Cost}{variable.Price.Currency}\nTotal: {variable.Price.Total }\nDescription: {variable.Description}\nPast Price: {variable.Price.Cost}{variable.PastPrice.Currency}\n");
            //}
        }

        public static async Task<IResult<IHtmlCollection<IElement>>> GetAllProductPageAsync(string address)
        {
            try
            {
                //await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });

                var page = await browser.NewPageAsync();

                await page.GoToAsync(address);

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

                using var document = await ParserContext.AngleSharpContext.OpenAsync(href);

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
