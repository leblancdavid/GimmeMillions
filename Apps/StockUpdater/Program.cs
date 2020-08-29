using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Stocks;
using System;

namespace StockUpdater
{
    class Program
    {
        static string _pathToStocks = "../../../../Repository/Stocks";
        static void Main(string[] args)
        {
            string watchlistFile = "../../../../Repository/volume-watchlist2.csv";
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), new PlaceholderStockHistoryRepository());
            var file = new System.IO.StreamReader(watchlistFile);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                var ticker = line.Split(',');
                Console.WriteLine($"Updating {ticker[0]}...");
                stocksRepo.UpdateStocks(ticker[0]);
            }
        }
    }
}
