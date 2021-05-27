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
        public StockRecommendation LastRecommendation
        {
            get
            {
                return _historicalData.LastOrDefault();
            }
        }

        public StockRecommendationHistory() { }
        public StockRecommendationHistory(string systemId, string symbol, IEnumerable<StockRecommendation> historicalData)
        {
            Symbol = symbol;
            SystemId = systemId;
            _historicalData = historicalData.ToList();
            _historicalDataStr = Stringify(historicalData);
            LastUpdated = DateTime.Now;
        }

        public void AddOrUpdateRecommendation(StockRecommendation recommendation)
        {
            if (recommendation.Symbol != Symbol || recommendation.SystemId != SystemId)
            {
                return;
            }

            var existingData = _historicalData.FirstOrDefault(x => x.Date.Date == recommendation.Date.Date);
            if(existingData != null)
                _historicalData.Remove(existingData);

            _historicalData.Add(recommendation);
            _historicalData = _historicalData.OrderBy(x => x.Date).ToList();
            _historicalDataStr = Stringify(_historicalData);
        }

        public StockRecommendationHistory(string systemId, string symbol, string dataStr)
        {
            Symbol = symbol;
            SystemId = systemId;
            _historicalDataStr = dataStr;
            _historicalData = Parse(symbol, dataStr).ToList();
            LastUpdated = DateTime.Now;
        }

        public bool ContainsEntryFor(DateTime date)
        {
            return _historicalData.Any(x => x.Date.Date == date.Date);
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
