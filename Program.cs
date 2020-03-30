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

            var shop = (await shopService.GetProductsAsync("https://www.facebook.com/pg/DyvenSvitShop")).Data;

            stopwatch.Stop();
            var stopwatchElapsed = stopwatch.Elapsed;

            Console.WriteLine("\tProducts\t\n ---------------------------\n ");

            //foreach (var variable in result)
            //{
            //    Console.WriteLine($"Name: {variable.Name}\nPrice: {variable.Price.Cost}{variable.Price.Currency}\nTotal: {variable.Price.Total }\nDescription: {variable.Description}\nPast Price: {variable.Price.Cost}{variable.PastPrice.Currency}\n");
            //}
        }
    }
}
