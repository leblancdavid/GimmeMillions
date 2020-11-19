namespace GimmeMillions.Domain.Stocks.Filters
{
    public interface IStockFilter
    {
        bool Pass(StockData stockData);
    }
}
