import 'package:gimmillions/models/stock-data.dart';

class StockRecommendation {
  DateTime date;
  String symbol;
  String systemId;
  double sentiment;
  double confidence;
  double prediction;
  double lowPrediction;
  double previousClose;
  double predictedPriceTarget;
  double predictedLowTarget;
  StockData stockData;

  StockRecommendation(
      this.date,
      this.symbol,
      this.systemId,
      this.sentiment,
      this.confidence,
      this.prediction,
      this.lowPrediction,
      this.previousClose,
      this.predictedPriceTarget,
      this.predictedLowTarget,
      this.stockData);
}
