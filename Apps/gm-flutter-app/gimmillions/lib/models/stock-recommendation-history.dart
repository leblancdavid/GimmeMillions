import 'package:gimmillions/models/stock-recommendation.dart';

class StockRecommendationHistory {
  DateTime lastUpdated;
  String symbol;
  String systemId;
  StockRecommendation lastRecommendation;
  List<StockRecommendation> historicalData;

  StockRecommendationHistory(this.systemId, this.symbol, this.lastUpdated,
      this.lastRecommendation, this.historicalData) {}
}
