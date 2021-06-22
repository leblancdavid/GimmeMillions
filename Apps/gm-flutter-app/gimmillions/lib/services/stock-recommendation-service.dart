import 'package:gimmillions/models/stock-data.dart';
import 'package:gimmillions/models/stock-recommendation.dart';

class StockRecommendationService {
  Future<List<StockRecommendation>> getFutures() {
    List<StockRecommendation> recommendations = [];

    recommendations.add(StockRecommendation(
        DateTime.now(),
        'DIA',
        'Test',
        42.24,
        0.42,
        11.11,
        22.22,
        33.33,
        44.44,
        55.55,
        StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));

    return Future.delayed(Duration(seconds: 2), () => recommendations);
  }

  Future<List<StockRecommendation>> getDailyPicks() {
    List<StockRecommendation> recommendations = List.empty();
    return Future.delayed(Duration(seconds: 2), () => recommendations);
  }

  Future<StockRecommendation> getRecommendationFor(String symbol) {
    StockRecommendation recommendation = new StockRecommendation(
        DateTime.now(),
        symbol,
        '',
        42,
        42,
        42,
        42,
        42,
        42,
        42,
        StockData(DateTime.now(), symbol, 42, 42, 42, 42, 42, 42, 42));
    return Future.delayed(Duration(seconds: 2), () => recommendation);
  }

  Future<StockRecommendation> getHistoryFor(String symbol) {
    StockRecommendation recommendation = new StockRecommendation(
        DateTime.now(),
        symbol,
        '',
        42,
        42,
        42,
        42,
        42,
        42,
        42,
        StockData(DateTime.now(), symbol, 42, 42, 42, 42, 42, 42, 42));
    return Future.delayed(Duration(seconds: 2), () => recommendation);
  }
}
