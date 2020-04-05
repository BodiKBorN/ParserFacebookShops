using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ParserFacebookShops.Services.Implementation
{
    public class ProductService : IProductService
    {
        private const string MagicSpace = " ";

        public IResult<Price> GetPrice(string htmlPrice, string pageLanguage)
        {
            try
            {
                if (htmlPrice == null)
                    return Result<Price>.CreateFailed("ELEMENT_NOT_FOUND");

                var htmlDecode = HttpUtility.HtmlDecode(htmlPrice);


                // Get current culture's NumberFormatInfo object.
                NumberFormatInfo nfi = CultureInfo.GetCultureInfo(pageLanguage).NumberFormat;

                // Assign needed property values to variables.
                bool symbolPrecedesIfPositive = nfi.CurrencyPositivePattern % 2 == 0;
                string groupSeparator = nfi.CurrencyGroupSeparator;
                string decimalSeparator = nfi.CurrencyDecimalSeparator;

                //string currencySymbol = nfi.CurrencySymbol;
                string currencySymbol = ParseCurrency(htmlPrice).Data;

                if (currencySymbol == "грн|грн.")
                    symbolPrecedesIfPositive = false;

                // Form regular expression pattern.
                string pattern = Regex.Escape(symbolPrecedesIfPositive ? currencySymbol + "." : "") +
                                 @"\s*" + "([0-9]{0,3}(" + groupSeparator + "[0-9]{3})*(" +
                                 Regex.Escape(decimalSeparator) + "[0-9]+)?)" + @"\s*" +
                                 Regex.Escape(!symbolPrecedesIfPositive ? currencySymbol + "." : "");

                string pattern2 = (symbolPrecedesIfPositive ? Regex.Escape(currencySymbol) + "\\.?" : "") +
                                @"\s*" + "([0-9]{0,3}(" + groupSeparator + "[0-9]{3})*(" + 
                                Regex.Escape(decimalSeparator) + "[0-9]+)?)" + @"\s*" + 
                                (!symbolPrecedesIfPositive ? Regex.Escape(currencySymbol) + "\\.?": "");
                

                // Get text that matches regular expression pattern.
                var value = Regex.Match(htmlPrice, pattern2, RegexOptions.IgnorePatternWhitespace);

                if (value.Value == string.Empty)
                    return Result<Price>.CreateFailed();

                var price = new Price();

                var cost = ParseCost(value.Groups[1].Value,nfi);

                if (cost.Success)
                    price.Cost = cost.Data;

                var currency = currencySymbol;

                //if (currency.Success)
                    price.Currency = currency;
                    price.Total = value.Value;
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

        private IResult<decimal> ParseCost(string htmlDecode, IFormatProvider nfi)
        {
            try
            {
                if (htmlDecode == null)
                    return Result<decimal>.CreateFailed("NOT_CORRECT_DATA");

                var value = htmlDecode;

                //var regex = new Regex(@"(\d+(\,|\.)?\d+)|(\d)");

                //var value = regex.Match(htmlDecode.Replace(MagicSpace, string.Empty)).Value;

                //var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();

                //numberFormat.NumberDecimalSeparator = new Regex(@"(\,|\.)").Match(value).Value;

                return decimal.TryParse(value, NumberStyles.Currency, nfi, out var cost)
                    ? Result<decimal>.CreateSuccess(cost)
                    : Result<decimal>.CreateFailed("COST_NOT_PARSE");
            }
            catch
            {
                return Result<decimal>.CreateFailed("PARSING_COST_ERROR");
            }
        }

        private IResult<string> ParseCurrency(string htmlDecode)
        {
            try
            {
                //var whiteSpaces = new[] { ' ', ' ' };
                //if (htmlDecode == null && !htmlDecode.Any(x => whiteSpaces.Contains(x)))
                //    Result<string>.CreateFailed("NOT_CORRECT_DATA");
                //var result = htmlDecode?.Substring(htmlDecode.LastIndexOfAny(whiteSpaces)).Trim();

                var regex = new Regex("(\\s?)([^\\d.,\\s ]+)\\1");

                var value = regex.Match(htmlDecode).Value;

                return Result<string>.CreateSuccess(value);
            }
            catch
            {
                return Result<string>.CreateFailed("PARSING_CURRENCY_ERROR");
            }
        }
    }
}