using System;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ParserFacebookShops.Services.Implementation
{
    public class ProductService : IProductService
    {
        private const string MagicSpace = " ";

        public IResult<Price> GetPrice(string htmlPrice)
        {
            try
            {
                if (htmlPrice == null)
                    return Result<Price>.CreateFailed("ELEMENT_NOT_FOUND");

                var htmlDecode = HttpUtility.HtmlDecode(htmlPrice);

                var price = new Price();

                price.Total = htmlDecode.Contains(MagicSpace) 
                    ? htmlDecode.Replace(MagicSpace, " ") 
                    : htmlDecode;

                var cost = ParseCost(htmlDecode);

                if (cost.Success)
                    price.Cost = cost.Data;

                var currency = ParseCurrency(htmlDecode);

                if (currency.Success)
                    price.Currency = currency.Data;

                return Result<Price>.CreateSuccess(price);
            }
            catch
            {
                return Result<Price>.CreateFailed("GETTING_PRICE_ERROR");
            }
        }

        public IResult<string> ParseImageUrl(string imageInnerText)
        {
            try
            {
                if (imageInnerText == null)
                    return Result<string>.CreateSuccess("NOT_CORRECT_DATA");

                var regex = new Regex("(http|https)+:\\/\\/\\S+[^\\\"\\)]");

                var match = regex.Match(imageInnerText).Value;

                return Result<string>.CreateSuccess(match);
            }
            catch (Exception e)
            {
                return Result<string>.CreateFailed("PARSING_IMAGE_URL_ERROR");
            }
        }

        private IResult<double> ParseCost(string htmlDecode)
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
                return Result<double>.CreateFailed("PARSING_COST_ERROR");
            }
        }

        private IResult<string> ParseCurrency(string htmlDecode)
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
                return Result<string>.CreateFailed("PARSING_CURRENCY_ERROR");
            }
        }
    }
}