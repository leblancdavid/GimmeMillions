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

  StockData(this.date, this.symbol, this.open, this.high, this.low, this.close, this.adjustedClose, this.volume,
      this.previousClose);

  double get percentChangeFromPreviousClose {
    if (this.previousClose == 0.0) {
      return 0.0;
    }
    return 100.0 * (close - previousClose) / previousClose;
  }

  factory StockData.fromJson(Map<String, dynamic> json) {
    return StockData(
        DateTime.parse(json['date']),
        json['symbol'],
        json['open'] as double,
        json['high'] as double,
        json['low'] as double,
        json['close'] as double,
        json['adjustedClose'] as double,
        json['volume'] as double,
        json['previousClose'] as double);
  }
}
