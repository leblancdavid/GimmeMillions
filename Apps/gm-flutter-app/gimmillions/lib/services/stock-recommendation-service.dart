import 'dart:convert';

import 'package:gimmillions/models/stock-recommendation-history.dart';
import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/services/authentication-service.dart';

import 'gimmillions-cache-manager.dart';

class StockRecommendationService {
  final AuthenticationService _authenticationService;

  StockRecommendationService(this._authenticationService);

  DateTime _getExpirationDateTime() {
    var currentTime = DateTime.now().toUtc();
    return DateTime.utc(currentTime.year, currentTime.month, currentTime.day, 0, 0);
  }

  Future<List<StockRecommendation>> getFutures() async {
    List<StockRecommendation> recommendations = [];
    var currentUser = _authenticationService.currentUser;
    if (currentUser == null || !currentUser.isLoggedIn || currentUser.authdata == '') {
      return recommendations;
    }

    final response = await CacheManagerInstance.instance.getSingleDatedFile(
        'http://api.gimmillions.com/api/recommendations/futures',
        headers: {'Authorization': 'Basic ${currentUser.authdata}'},
        expiration: _getExpirationDateTime());

    if (await response.exists()) {
      var json = jsonDecode(await response.readAsString()) as List;
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

    final response = await CacheManagerInstance.instance.getSingleDatedFile(
        'http://api.gimmillions.com/api/recommendations/stocks/daily',
        headers: {'Authorization': 'Basic ${currentUser.authdata}'},
        expiration: _getExpirationDateTime());

    if (await response.exists()) {
      var json = jsonDecode(await response.readAsString()) as List;
      json.forEach((element) {
        recommendations.add(StockRecommendation.fromJson(element));
      });
      return recommendations;
    } else {
      throw Exception('Unable to retrieve daily predictions');
    }
  }

  Future<StockRecommendationHistory> getHistoryFor(String symbol) async {
    var currentUser = _authenticationService.currentUser;
    if (currentUser == null || !currentUser.isLoggedIn || currentUser.authdata == '') {
      return Future.error('User is not logged in');
    }

    final response = await CacheManagerInstance.instance.getSingleDatedFile(
        'http://api.gimmillions.com/api/recommendations/stocks/history/${symbol}',
        headers: {'Authorization': 'Basic ${currentUser.authdata}'},
        expiration: _getExpirationDateTime());

    if (await response.exists()) {
      var json = jsonDecode(await response.readAsString());

      return StockRecommendationHistory.fromJson(json);
    } else {
      throw Exception('Unable to retrieve history data for ${symbol}');
    }
  }
}
