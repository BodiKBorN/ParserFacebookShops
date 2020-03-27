using AngleSharp;
using AngleSharp.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace ParserFacebookShops.Services.Implementation
{
    public class ProductService : IProductService
    {
        private const string MagicSpace = " ";

        public async Task<IResult<Product>> GetFullProductCardAsync(IElement element)
        {
            try
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " in get product");
                if (element == null)
                    Result<Product>.CreateFailed();

                var querySelector = element.QuerySelector("div > div > div > a");

                if (!(querySelector is IHtmlAnchorElement))
                    return Result<Product>.CreateFailed();

                var cardUrl = (querySelector as IHtmlAnchorElement).Href;
                

                using var document = await ParserContext.AngleSharpContext.OpenAsync(cardUrl);

                await document.WaitForReadyAsync();

                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " in get product ----past");
                var documentBody = document.Body;

                return null;
            }
            catch (Exception e)
            {
                return Result<Product>.CreateFailed();
            }
        }

        public string GetName(IElement element) =>
            element.QuerySelector("div > div > div > a > strong")?.InnerHtml;

        public IResult<Price> ParsePrice(string htmlPrice)
        {
            try
            {
                if (htmlPrice == null)
                    return Result<Price>.CreateFailed("ELEMENT_NOT_FOUND");

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

        private IResult GetImage()
        {
            throw new System.NotImplementedException();
        }

        private IResult GetDescription()
        {
            throw new System.NotImplementedException();
        }

        private IResult<double> GetCost(string htmlDecode)
        {
            try
            {
                if (htmlDecode == null)
                    return Result<double>.CreateFailed("NOT_CORRECT_DATA");

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

        private IResult<string> GetCurrency(string htmlDecode)
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
    }
}