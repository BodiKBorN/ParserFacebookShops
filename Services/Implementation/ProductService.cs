using ParserFacebookShops.Models;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ParserFacebookShops.Services.Implementation
{
    public class ProductService : IProductService
    {
        public IResult<Price> GetPrice(string htmlPrice, string pageLanguage)
        {
            try
            {
                if (htmlPrice == null)
                    return Result<Price>.CreateFailed("ELEMENT_NOT_FOUND");

                // Get current culture's NumberFormatInfo object.
                NumberFormatInfo nfi = pageLanguage != null
                    ? CultureInfo.GetCultureInfo(pageLanguage).NumberFormat
                    : CultureInfo.CurrentCulture.NumberFormat;

                // Assign needed property values to variables.
                var symbolPrecedesIfPositive = nfi.CurrencyPositivePattern % 2 == 0;
                var groupSeparator = nfi.CurrencyGroupSeparator;
                var decimalSeparator = nfi.CurrencyDecimalSeparator;

                //string currencySymbol = provider.CurrencySymbol;
                var parseCurrency = ParseCurrency(htmlPrice);

                var currencySymbol = parseCurrency.Success
                    ? parseCurrency.Data
                    : nfi.CurrencySymbol;

                if (currencySymbol.Contains("грн"))
                    symbolPrecedesIfPositive = false;

                // Form regular expression pattern.
                var pattern = (symbolPrecedesIfPositive ? Regex.Escape(currencySymbol) + "\\.?" : "") +
                                  @"\s*" + "([0-9]{0,3}(" + groupSeparator + "[0-9]{3})*(" +
                                  Regex.Escape(decimalSeparator) + "[0-9]+)?)" + @"\s*" +
                                  (!symbolPrecedesIfPositive ? Regex.Escape(currencySymbol) + "\\.?" : "");

                // Get text that matches regular expression pattern.
                var value = Regex.Match(htmlPrice, pattern, RegexOptions.IgnorePatternWhitespace);

                if (value.Value.Equals(string.Empty))
                    return Result<Price>.CreateFailed("PRICE_NOT_MATCH");

                var cost = ParseCost(value.Groups[1].Value, nfi);

                if (!cost.Success)
                    return Result<Price>.CreateFailed(cost.Message);

                var price = new Price
                {
                    Cost = cost.Data,
                    Currency = currencySymbol,
                    Total = value.Value
                };

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
            catch
            {
                return Result<string>.CreateFailed("PARSING_IMAGE_URL_ERROR");
            }
        }

        private IResult<decimal> ParseCost(string value, IFormatProvider provider)
        {
            try
            {
                if (value == null || value.Equals(string.Empty))
                    return Result<decimal>.CreateFailed("NOT_CORRECT_DATA");

                return decimal.TryParse(value, NumberStyles.Currency, provider, out var cost)
                    ? Result<decimal>.CreateSuccess(cost)
                    : Result<decimal>.CreateFailed("COST_NOT_PARSE");
            }
            catch
            {
                return Result<decimal>.CreateFailed("PARSING_COST_ERROR");
            }
        }

        private IResult<string> ParseCurrency(string htmlPrice)
        {
            try
            {
                var regex = new Regex("(\\s?)([^\\d.,\\s ]+)\\1");

                var value = regex.Match(htmlPrice).Value;

                return Result<string>.CreateSuccess(value);
            }
            catch
            {
                return Result<string>.CreateFailed("PARSING_CURRENCY_ERROR");
            }
        }
    }
}