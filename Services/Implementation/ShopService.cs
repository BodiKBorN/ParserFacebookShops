using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParserFacebookShops.Models.Implementation;

namespace ParserFacebookShops.Services.Implementation
{
    public class ShopService : IShopService
    {
        private readonly IProductService _productService;
        private readonly AngleSharpParser _angleSharpParser;
        public ShopService()
        {
            _productService = new ProductService();
            _angleSharpParser = new AngleSharpParser();
        }

        public async Task<IResult<List<Product>>> GetProductsAsync(string shopId)
        {
            try
            {
                if (!shopId.EndsWith("/"))
                    shopId += "/";

                shopId += "shop/";

                var elementsFromShopPage = await _angleSharpParser.GetElementsFromShopPageAsync(shopId);

                var productElements = elementsFromShopPage.Length != 0
                    ? elementsFromShopPage
                    : await _angleSharpParser.GetElementsFromAllProductPageAsync(shopId);

                if (productElements.Length == 0)
                    return Result<List<Product>>.CreateFailed("ELEMENTS_NOT_FOUND");

                var productModels = _angleSharpParser.GetProducts(productElements);

                var whenAll = await Task.WhenAll(productModels);

                return Result<List<Product>>.CreateSuccess(whenAll.ToList());
            }
            catch (Exception e)
            {
                return Result<List<Product>>.CreateFailed("ERROR_GET_PRODUCTS");
            }
        }
    }
}