using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParserFacebookShops.Services.Implementation
{
    public class ParserService : IParserService
    {
        private readonly IShopService _shopService;
        public ParserService()
        {
            _shopService = new ShopService();
        }

        public async Task<IResult<List<Product>>> ParseShopAsync(string shopId, bool authentication = false)
        {
            if (authentication)
            {
                var resultAuthentication = await SetAuthentication();

                if (!resultAuthentication.Success)
                    return Result<List<Product>>.CreateFailed("PARSE_SHOP_NOT_AUTHENTICATION");
            }

            var result = await _shopService.GetProductsAsync(shopId);

            if (!result.Success)
                return Result<List<Product>>.CreateFailed("ERROR_PARSE_SHOP");


            return Result<List<Product>>.CreateSuccess(result.Data);
        }

        private async Task<IResult> SetAuthentication()
        {
            try
            {
                //Implementation authentication with AngleSharp
                ParserContext.AngleSharpContext.OpenAsync("shopId").Wait();
                (ParserContext.AngleSharpContext.Active.QuerySelector<IHtmlInputElement>("input#email")).Value = "bodik_kz@ukr.net";
                (ParserContext.AngleSharpContext.Active.QuerySelector<IHtmlInputElement>("input#pass")).Value = "201001chepa";

                await (ParserContext.AngleSharpContext.Active.QuerySelector("#loginbutton").Children.FirstOrDefault() as IHtmlInputElement)
                    .SubmitAsync();

                return Result.CreateSuccess();
            }
            catch (Exception e)
            {
                return Result.CreateFailed("AUTHENTICATION_ERROR");
            }
        }
    }
}