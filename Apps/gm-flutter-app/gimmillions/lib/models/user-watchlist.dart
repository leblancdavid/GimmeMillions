import 'dart:convert';

import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/models/user.dart';
import 'package:gimmillions/services/gimmillions-cache-manager.dart';
import 'package:http/http.dart' as http;

class UserWatchlist {
  late List<StockRecommendation> _watchlist;
  late User currentUser;

  UserWatchlist() {
    _watchlist = [];
  }

  Future<List<StockRecommendation>> get watchlist {
    return Future.value(_watchlist);
  }

  Future<void> addToWatchlist(StockRecommendation r) async {
    this._watchlist.add(r);

    var body = jsonEncode({
      'username': currentUser.username,
      'symbols': [r.symbol]
    });
    final response = await http.put(Uri.parse('http://api.gimmillions.com/api/user/watchlist/add'),
        headers: <String, String>{
          'Content-Type': 'application/json; charset=UTF-8',
          'Authorization': 'Basic ${currentUser.authdata}'
        },
        body: body);

    if (response.statusCode != 200) {
      throw Exception('Unable to update user watchlist');
    }

    return await CacheManagerInstance.instance
        .removeFile('http://api.gimmillions.com/api/recommendations/stocks/user/${currentUser.username}');
  }

  Future<void> removeFromWatchlist(String symbol) async {
    this._watchlist.removeWhere((element) => element.symbol == symbol);
    final response = await http.put(Uri.parse('http://api.gimmillions.com/api/user/watchlist/remove'),
        headers: <String, String>{
          'Content-Type': 'application/json; charset=UTF-8',
          'Authorization': 'Basic ${currentUser.authdata}'
        },
        body: jsonEncode({
          'username': currentUser.username,
          'symbols': [symbol]
        }));

    if (response.statusCode != 200) {
      throw Exception('Unable to update user watchlist');
    }

    return await CacheManagerInstance.instance
        .removeFile('http://api.gimmillions.com/api/recommendations/stocks/user/${currentUser.username}');
  }

  bool containsSymbol(String symbol) {
    return this._watchlist.any((element) => element.symbol.toLowerCase() == symbol.toLowerCase());
  }

  Future<List<StockRecommendation>> refresh() async {
    _watchlist = [];
    if (!currentUser.isLoggedIn || currentUser.authdata == '') {
      return _watchlist;
    }

    final response = await CacheManagerInstance.instance.getSingleDatedFile(
        'http://api.gimmillions.com/api/recommendations/stocks/user/${currentUser.username}',
        headers: {'Authorization': 'Basic ${currentUser.authdata}'},
        expiration: _getExpirationDateTime());

    if (await response.exists()) {
      var json = jsonDecode(await response.readAsString()) as List;
      json.forEach((element) {
        _watchlist.add(StockRecommendation.fromJson(element));
      });

      return watchlist;
    } else {
      throw Exception('Unable to retrieve user watchlist');
    }
  }

  DateTime _getExpirationDateTime() {
    var currentTime = DateTime.now().toUtc();
    return DateTime.utc(currentTime.year, currentTime.month, currentTime.day, 0, 0);
  }
}
