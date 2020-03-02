using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimulationTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var rng = new Random();
            string _pathToStocks = "../../../../Repository/Stocks";
            var repo = new StockDataRepository(_pathToStocks);

            var startDate = new DateTime(2018, 1, 1);
            var endDate = new DateTime(2019, 1, 1);
            var stocks = repo.GetStocks("IWM")
                .Where(x => x.Date >= startDate && x.Date <= endDate)
                .ToList();

            if(!stocks.Any())
            {
                return;
            }

            var startingPrice = stocks.First().Open;
            var endingPrice = stocks.Last().Open;
            var totalPercentChange = (endingPrice - startingPrice) / startingPrice;
            Console.WriteLine($"Percent change from {startDate.ToString("mm/dd/yyyy")} to {endDate.ToString("mm/dd/yyyy")} is {totalPercentChange * 100m}%");

            var averagePercentDay = stocks.Select(x => x.PercentDayChange).Average();
            Console.WriteLine($"The average percent day change is {averagePercentDay}%");

            //Investing all the time
            //var endingMoney = RunInvestingAllTheTime(1000m, stocks);
            //Console.WriteLine($"Starting with $1000, when investing every day, you would end up with {endingMoney}");

            for (decimal predictorAccuracy = 0.45m; predictorAccuracy <= 1.0m; predictorAccuracy += 0.01m)
            {
                var endingMoney = 0m;
                for (int i = 0; i < 100; ++i)
                {
                    endingMoney += RunWithPredictorMethod(1000m, predictorAccuracy, stocks, rng);
                }
                endingMoney /= 100m;
                Console.WriteLine($"Starting with $1000, when investing a {predictorAccuracy}, you would end up on average with ${endingMoney}");
            }

            Console.ReadLine();
            

        }

        static decimal RunInvestingAllTheTime(decimal startingMoney, IEnumerable<StockData> stocks)
        {
            decimal currentMoney = startingMoney;
            foreach (var stock in stocks)
            {
                currentMoney = currentMoney * (1.0m + stock.PercentDayChange);
            }

            return currentMoney;
        }

        static decimal RunWithPredictorMethod(decimal startingMoney, decimal correctnessFactor, 
            IEnumerable<StockData> stocks, Random rng)
        {
            decimal currentMoney = startingMoney;
            foreach (var stock in stocks)
            {
                if (currentMoney < 0.001m)
                    break;
                var r = (decimal)rng.NextDouble();
                if(r < correctnessFactor)
                {
                    if(stock.PercentDayChange > 0)
                    {
                        currentMoney = currentMoney * (1.0m + stock.PercentDayChange / 100m);
                    }
                }
                else
                {
                    if(stock.PercentDayChange < 0)
                        currentMoney = currentMoney * (1.0m + stock.PercentDayChange / 100m);
                }
            }

            return currentMoney;
        }
    }
}
