import 'package:gimmillions/models/stock-data.dart';
import 'package:gimmillions/models/stock-recommendation-history.dart';
import 'package:gimmillions/models/stock-recommendation.dart';

class StockRecommendationService {
  Future<List<StockRecommendation>> getFutures() {
    List<StockRecommendation> recommendations = [];

    recommendations.add(StockRecommendation(DateTime.now(), 'DIA', 'Test', 50.0, 0.42, 11.11, 22.22, 33.33, 44.44,
        55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));

    recommendations.add(StockRecommendation(DateTime.now(), 'QQQ', 'Test', 75, 0.77, 11.11, 22.22, 33.33, 44.44, 55.55,
        StockData(DateTime.now(), 'QQQ', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));

    recommendations.add(StockRecommendation(DateTime.now(), 'SPY', 'Test', 100, 0.22, 11.11, 22.22, 33.33, 44.44, 55.55,
        StockData(DateTime.now(), 'SPY', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));

    recommendations.add(StockRecommendation(DateTime.now(), 'RUT', 'Test', 0, -0.77, 11.11, 22.22, 33.33, 44.44, 55.55,
        StockData(DateTime.now(), 'RUT', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));
    return Future.delayed(Duration(seconds: 2), () => recommendations);
  }

  Future<List<StockRecommendation>> getDailyPicks() {
    List<StockRecommendation> recommendations = List.empty();
    return Future.delayed(Duration(seconds: 2), () => recommendations);
  }

  Future<StockRecommendation> getRecommendationFor(String symbol) {
    StockRecommendation recommendation = new StockRecommendation(DateTime.now(), symbol, '', 42, 42, 42, 42, 42, 42, 42,
        StockData(DateTime.now(), symbol, 42, 42, 42, 42, 42, 42, 42));
    return Future.delayed(Duration(seconds: 2), () => recommendation);
  }

  Future<StockRecommendationHistory> getHistoryFor(String symbol) {
    List<StockRecommendation> recommendations = [];

    recommendations.add(StockRecommendation(DateTime(2021, 6, 13), 'DIA', 'Test', 76.0, 0.42, 11.11, 22.22, 33.33,
        44.44, 55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));
    recommendations.add(StockRecommendation(DateTime(2021, 6, 14), 'DIA', 'Test', 55.0, 0.42, 11.11, 22.22, 33.33,
        44.44, 55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));
    recommendations.add(StockRecommendation(DateTime(2021, 6, 15), 'DIA', 'Test', 45.0, 0.42, 11.11, 22.22, 33.33,
        44.44, 55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));
    recommendations.add(StockRecommendation(DateTime(2021, 6, 16), 'DIA', 'Test', 42.0, 0.42, 11.11, 22.22, 33.33,
        44.44, 55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));
    recommendations.add(StockRecommendation(DateTime(2021, 6, 17), 'DIA', 'Test', 33.0, 0.42, 11.11, 22.22, 33.33,
        44.44, 55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));

    recommendations.add(StockRecommendation(DateTime(2021, 6, 18), 'DIA', 'Test', 25.0, 0.42, 11.11, 22.22, 33.33,
        44.44, 55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));
    recommendations.add(StockRecommendation(DateTime(2021, 6, 19), 'DIA', 'Test', 12.0, 0.42, 11.11, 22.22, 33.33,
        44.44, 55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));
    recommendations.add(StockRecommendation(DateTime(2021, 6, 20), 'DIA', 'Test', 22.0, 0.42, 11.11, 22.22, 33.33,
        44.44, 55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));
    recommendations.add(StockRecommendation(DateTime(2021, 6, 21), 'DIA', 'Test', 35.0, 0.42, 11.11, 22.22, 33.33,
        44.44, 55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));

    recommendations.add(StockRecommendation(DateTime(2021, 6, 22), 'DIA', 'Test', 65, 0.77, 11.11, 22.22, 33.33, 44.44,
        55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));

    recommendations.add(StockRecommendation(DateTime(2021, 6, 23), 'DIA', 'Test', 86, 0.22, 11.11, 22.22, 33.33, 44.44,
        55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));

    recommendations.add(StockRecommendation(DateTime(2021, 6, 24), 'DIA', 'Test', 73, -0.77, 11.11, 22.22, 33.33, 44.44,
        55.55, StockData(DateTime.now(), 'DIA', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)));

    StockRecommendationHistory history =
        StockRecommendationHistory('systemId', 'DIA', DateTime.now(), recommendations.last, recommendations);
    return Future.delayed(Duration(seconds: 2), () => history);
  }
}
