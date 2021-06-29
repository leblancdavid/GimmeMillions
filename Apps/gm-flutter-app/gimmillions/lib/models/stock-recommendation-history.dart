import 'package:gimmillions/models/stock-recommendation.dart';

class StockRecommendationHistory {
  DateTime lastUpdated;
  String symbol;
  String systemId;
  StockRecommendation lastRecommendation;
  List<StockRecommendation> historicalData;

  StockRecommendationHistory(
      this.systemId, this.symbol, this.lastUpdated, this.lastRecommendation, this.historicalData) {}

  factory StockRecommendationHistory.fromJson(Map<String, dynamic> json) {
    var historicalData = json['historicalData'] as List;

    return StockRecommendationHistory(
        json['systemId'],
        json['symbol'],
        DateTime.parse(json['lastUpdated']),
        StockRecommendation.fromJson(json['lastRecommendation']),
        historicalData.map((e) => StockRecommendation.fromJson(e)).toList());
  }
}
