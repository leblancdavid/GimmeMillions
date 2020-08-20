using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.SQLDataAccess
{
    public class SQLStockHistoryRepository : IStockHistoryRepository
    {
        private readonly DbContextOptions<GimmeMillionsContext> _dbContextOptions;
        public SQLStockHistoryRepository(DbContextOptions<GimmeMillionsContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public Result AddOrUpdateStock(StockHistory stockData)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);

                var stock = context.StockHistories.FirstOrDefault(x => x.Symbol == stockData.Symbol);
                if (stock == null)
                {
                    context.StockHistories.Add(stockData);
                }
                else
                {
                    stock.HistoricalDataStr = stockData.HistoricalDataStr;
                }
                context.SaveChanges();
                return Result.Ok();
            }
            catch(Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public void Delete(string symbol)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            context.StockHistories.RemoveRange(context.StockHistories.Where(x => x.Symbol == symbol));
            context.SaveChanges();
        }

        public Result<StockHistory> GetStock(string symbol)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            var stock = context.StockHistories.FirstOrDefault(x => x.Symbol == symbol);
            if(stock == null)
            {
                return Result.Failure<StockHistory>($"Cannot find stock {symbol} in database");
            }

            stock.LoadData();
            return Result.Ok<StockHistory>(stock);
        }

        public IEnumerable<StockHistory> GetStocks()
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            var stocks = context.StockHistories.ToList();
            stocks.ForEach(x => x.LoadData());
            return stocks;
        }

        public IEnumerable<string> GetSymbols()
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            return context.StockHistories.Select(x => x.Symbol);
        }
    }
}
