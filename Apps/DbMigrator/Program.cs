using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.SQLDataAccess;
using Microsoft.EntityFrameworkCore;
using System;

namespace DbMigrator
{
    class Program
    {
        static string _pathToRepository = "C:\\Databases";
        static string _pathToStocks = "../../../../../Repository/Stocks";
        static void Main(string[] args)
        {
            var stockFileRepository = new StockDataRepository(_pathToStocks); 
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            optionsBuilder.UseSqlite($"Data Source=C:/Databases/gm.db");

            var context = new GimmeMillionsContext(optionsBuilder.Options);
            //context.Database.EnsureCreated();
            context.Database.Migrate();

            var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);

            var symbols = stockFileRepository.GetSymbols();
            foreach (var symbol in symbols)
            {
                var stockData = stockFileRepository.GetStocks(symbol);
                var stockHistory = new StockHistory(symbol, stockData);
                stockSqlDb.AddOrUpdateStock(stockHistory);

                var getTest = stockSqlDb.GetStock(symbol);
            }
        }
    }
}
