namespace GimmeMillions.Domain.Stocks
{
    public interface IStockFilter
    {
        bool Pass(StockData stockData);
    }
}
