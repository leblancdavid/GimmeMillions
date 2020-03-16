using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public class StockRecommendationSystemConfiguration
    {
        public List<(string Symbol, string PathToModel, string Encoding)> Models { get; set; }

        public StockRecommendationSystemConfiguration()
        {
            Models = new List<(string Symbol, string PathToModel, string Encoding)>();
        }

    }
}
