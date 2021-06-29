import 'dart:convert';

import 'package:gimmillions/models/stock-data.dart';
import 'package:gimmillions/models/stock-recommendation-history.dart';
import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/services/authentication-service.dart';
import 'package:http/http.dart' as http;

class StockRecommendationService {
  final AuthenticationService _authenticationService;

  StockRecommendationService(this._authenticationService);

  Future<List<StockRecommendation>> getFutures() async {
    List<StockRecommendation> recommendations = [];
    var currentUser = _authenticationService.currentUser;
    if (currentUser == null || !currentUser.isLoggedIn || currentUser.authdata == '') {
      return recommendations;
    }

    final response = await http.get(Uri.parse('http://api.gimmillions.com/api/recommendations/futures'),
        headers: {'Authorization': 'Basic ${currentUser.authdata}'});

    if (response.statusCode == 200) {
      var json = jsonDecode(response.body) as List;
      json.forEach((element) {
        recommendations.add(StockRecommendation.fromJson(element));
      });
      return recommendations;
    } else {
      throw Exception('Unable to retrieve future predictions');
    }
  }

  Future<List<StockRecommendation>> getDailyPicks() async {
    List<StockRecommendation> recommendations = [];
    var currentUser = _authenticationService.currentUser;
    if (currentUser == null || !currentUser.isLoggedIn || currentUser.authdata == '') {
      return recommendations;
    }

    final response = await http.get(Uri.parse('http://api.gimmillions.com/api/recommendations/stocks/daily'),
        headers: {'Authorization': 'Basic ${currentUser.authdata}'});

    if (response.statusCode == 200) {
      var json = jsonDecode(response.body) as List;
      json.forEach((element) {
        recommendations.add(StockRecommendation.fromJson(element));
      });
      return recommendations;
    } else {
      throw Exception('Unable to retrieve daily predictions');
    }
  }

  Future<List<StockRecommendation>> getUserWatchlist() {
    List<StockRecommendation> recommendations = List.empty();
    return Future.delayed(Duration(milliseconds: 100), () => recommendations);
  }

  Future<StockRecommendation> getRecommendationFor(String symbol) {
    StockRecommendation recommendation = new StockRecommendation(DateTime.now(), symbol, '', 42, 42, 42, 42, 42, 42, 42,
        StockData(DateTime.now(), symbol, 42, 42, 42, 42, 42, 42, 42));
    return Future.delayed(Duration(milliseconds: 100), () => recommendation);
  }

  Future<StockRecommendationHistory> getHistoryFor(String symbol) async {
    var currentUser = _authenticationService.currentUser;
    if (currentUser == null || !currentUser.isLoggedIn || currentUser.authdata == '') {
      return Future.error('User is not logged in');
    }

    final response = await http.get(
        Uri.parse('http://api.gimmillions.com/api/recommendations/stocks/history/${symbol}'),
        headers: {'Authorization': 'Basic ${currentUser.authdata}'});

    if (response.statusCode == 200) {
      var json = jsonDecode(response.body);

      return StockRecommendationHistory.fromJson(json);
    } else {
      throw Exception('Unable to retrieve history data for ${symbol}');
    }
  }
}
