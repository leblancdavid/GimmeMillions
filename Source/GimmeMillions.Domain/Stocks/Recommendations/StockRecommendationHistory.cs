using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.Stocks.Recommendations
{
    public class StockRecommendationHistory
    {
        public int Id { get; set; }
        public string SystemId { get; private set; }
        public string Symbol { get; set; }
        private List<StockRecommendation> _historicalData = new List<StockRecommendation>();
        public IEnumerable<StockRecommendation> HistoricalData => _historicalData;

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

        public StockRecommendationHistory() { }
        public StockRecommendationHistory(string systemId, string symbol, IEnumerable<StockRecommendation> historicalData)
        {
            Symbol = symbol;
            SystemId = systemId;
            _historicalData = historicalData.ToList();
            _historicalDataStr = Stringify(historicalData);
            LastUpdated = DateTime.Now;
        }

        public void AddRecommendation(StockRecommendation recommendation)
        {
            if(_historicalData.Any(x => x.Date.Date == recommendation.Date.Date))
            {
                _historicalData.Add(recommendation);
                _historicalData = _historicalData.OrderBy(x => x.Date).ToList();
            }
        }

        public StockRecommendationHistory(string systemId, string symbol, string dataStr)
        {
            Symbol = symbol;
            SystemId = systemId;
            _historicalDataStr = dataStr;
            _historicalData = Parse(symbol, dataStr).ToList();
            LastUpdated = DateTime.Now;
        }

        public void LoadData()
        {
            _historicalData = Parse(Symbol, _historicalDataStr).ToList();
        }

        public static string Stringify(IEnumerable<StockRecommendation> historicalData)
        {
            return JsonConvert.SerializeObject(historicalData);
        }

        public static IEnumerable<StockRecommendation> Parse(string symbol, string dataStr)
        {
            var historicalData = new List<StockRecommendation>();

            try
            {
                historicalData = JsonConvert.DeserializeObject<List<StockRecommendation>>(dataStr);
            }
            catch (Exception)
            {

            }

            return historicalData;
        }

    }
}
