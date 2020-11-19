using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public class StockHistory
    {

        public int Id { get; set; }
        public string Symbol { get; set; }
        private List<StockData> _historicalData = new List<StockData>();
        public IEnumerable<StockData> HistoricalData => _historicalData;

        private string _historicalDataStr;
        public string HistoricalDataStr 
        {
            get
            {
                return _historicalDataStr;
            }
            set
            {
                _historicalData = Parse(Symbol, value).ToList();
                _historicalDataStr = Stringify(_historicalData);
            }
        }

        public DateTime LastUpdated { get; set; }

        public StockHistory() { }
        public StockHistory(string symbol, IEnumerable<StockData> historicalData)
        {
            Symbol = symbol;
            _historicalData = historicalData.ToList();
            _historicalDataStr = Stringify(historicalData);
            LastUpdated = DateTime.Now;
        }

        public StockHistory(string symbol, string dataStr)
        {
            Symbol = symbol;
            _historicalDataStr = dataStr;
            _historicalData = Parse(symbol, dataStr).ToList();
            LastUpdated = DateTime.Now;
        }

        public void LoadData()
        {
            _historicalData = Parse(Symbol, _historicalDataStr).ToList();
        }

        public static string Stringify(IEnumerable<StockData> historicalData)
        {
            string historyStr = "Date,Open,High,Low,Close,Adj Close,Volume\n";
            foreach(var data in historicalData)
            {
                historyStr += $"{data.Date.ToString("yyyy-MM-dd")},{data.Open},{data.High},{data.Low},{data.Close},{data.AdjustedClose},{data.Volume}\n";
            }

            return historyStr;
        }

        public static IEnumerable<StockData> Parse(string symbol, string dataStr)
        {
            var historicalData = new List<StockData>();

            try
            {
                var lines = dataStr.Split('\n');
                StockData previous = null;
                foreach (var line in lines)
                {
                    var fields = line.Split(',');
                    DateTime date;
                    decimal open, high, low, close, adjustedClose, volume;
                    if (DateTime.TryParse(fields[0], out date) &&
                        decimal.TryParse(fields[1], out open) &&
                        decimal.TryParse(fields[2], out high) &&
                        decimal.TryParse(fields[3], out low) &&
                        decimal.TryParse(fields[4], out close) &&
                        decimal.TryParse(fields[5], out adjustedClose) &&
                        decimal.TryParse(fields[6], out volume))
                    {
                        var stock = new StockData(symbol, date, open, high, low, close, adjustedClose, volume);
                        if (previous != null)
                        {
                            stock.PreviousClose = previous.Close;
                        }
                        historicalData.Add(stock);
                        previous = stock;
                    }
                }
            }
            catch(Exception ex)
            {

            }


            return historicalData;
        }

    }
}
