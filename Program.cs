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
    class Program : BaseLoader
    {
        public Program(IBrowsingContext context, Predicate<Request> filter) : base(context, filter)
        {
            context = WebContext.Context;
        }

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

                //Implementation authentication
                //WebContext.Context.OpenAsync("https://www.facebook.com").Wait();
                //(WebContext.Context.Active.QuerySelector<IHtmlInputElement>("input#email")).Value = "bodik_kz@ukr.net";
                //(WebContext.Context.Active.QuerySelector<IHtmlInputElement>("input#pass")).Value = "201001chepa";
                //await (WebContext.Context.Active.QuerySelector("#loginbutton").Children.FirstOrDefault() as IHtmlInputElement)
                //    .SubmitAsync();

                if (!shopUrl.EndsWith("/"))
                    shopUrl += "/";

                var document = await WebContext.Context.OpenAsync("https://www.facebook.com/pg/nusho.shop/shop/?rt=9&ref=page_internal&cid=508067916290424");
                await document.WaitForReadyAsync();

                var querySelector = document.QuerySelectorAll("#content_container table > tbody > tr td");

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
            if (cardUrl == null)
                Result<IElement>.CreateFailed();

            var openAsync = await WebContext.Context.OpenAsync(cardUrl);
            return null;
        }
    }
}
