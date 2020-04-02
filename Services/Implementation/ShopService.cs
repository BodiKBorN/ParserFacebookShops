using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParserFacebookShops.Services.Implementation
{
    public class ShopService : IShopService, IDisposable
    {

        private readonly AngleSharpParser _angleSharpParser;
        public ShopService()
        {
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

                var productElements = elementsFromShopPage.Data.Length != 0
                    ? elementsFromShopPage
                    : (await _angleSharpParser.GetElementsFromAllProductPageAsync(shopId));

                if (!productElements.Success && productElements.Data.Length == 0)
                    return Result<List<Product>>.CreateFailed(productElements.Message);

                var productModels = _angleSharpParser.GetProducts(productElements.Data);

                if (!productModels.Success)
                    return Result<List<Product>>.CreateFailed(productModels.Message);

                var products = await Task.WhenAll(productModels.Data);

                return Result<List<Product>>.CreateSuccess(products.ToList());
            }
            catch(Exception e)
            {
                return Result<List<Product>>.CreateFailed("ERROR_GET_PRODUCTS");
            }
        }

        public void Dispose()
        {
            _angleSharpParser?.Dispose();
        }
    }
}