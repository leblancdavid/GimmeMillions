import 'package:gimmillions/models/stock-data.dart';
import 'package:gimmillions/models/stock-recommendation.dart';

class StockRecommendationService {
  Future<List<StockRecommendation>> getFutures() {
    List<StockRecommendation> recommendations = List.empty();
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
