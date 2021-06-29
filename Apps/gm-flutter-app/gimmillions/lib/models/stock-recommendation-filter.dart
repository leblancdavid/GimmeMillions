import 'package:flutter/cupertino.dart';
import 'package:gimmillions/models/stock-recommendation.dart';

class StockRecommendationFilter extends ChangeNotifier {
  String _symbolFilter = '';

  String get symbolFilter {
    return _symbolFilter;
  }

  set symbolFilter(String value) {
    _symbolFilter = value.toLowerCase();
    notifyListeners();
  }

  List<StockRecommendation> filter(List<StockRecommendation> recommendations) {
    if (_symbolFilter == '') {
      return recommendations;
    }
    return recommendations.where((r) => r.symbol.toLowerCase().contains(_symbolFilter)).toList();
  }
}
