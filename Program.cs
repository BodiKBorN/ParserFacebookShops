using ParserFacebookShops.Services.Implementation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ParserFacebookShops
{
    class Program
    {
        static async Task Main()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using var shopService = new ShopService("bohdanmanager.prland@gmail.com", "PRland1234");

            var shop = (await shopService.GetProductsAsync("https://www.facebook.com/pg/Shop-and-Go-105766151037772"));

            var shopProducts = shop.Data;

            stopwatch.Stop();

            var stopwatchElapsed = stopwatch.Elapsed;

            Console.WriteLine("\tProducts\t\n ---------------------------\n ");

            foreach (var variable in shopProducts)
            {
                Console.WriteLine($"Name: {variable.Name}\nPrice: {variable.Price?.Cost}{variable.Price?.Currency}\nTotal: {variable.Price?.Total }\nDescription: {variable.Description}\nImage: {variable.ImagesUrl}\n");
            }

            Console.ReadKey();
        }
    }
}
