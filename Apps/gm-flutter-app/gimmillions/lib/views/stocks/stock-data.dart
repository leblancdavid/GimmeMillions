class StockData {
  DateTime date;
  String symbol;
  double open;
  double high;
  double low;
  double close;
  double adjustedClose;
  double volume;
  double previousClose;

  StockData(this.date, this.symbol, this.open, this.high, this.low, this.close,
      this.adjustedClose, this.volume, this.previousClose);
}
