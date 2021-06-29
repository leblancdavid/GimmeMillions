import 'dart:convert';

import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/models/user.dart';
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

    final response = await http.put(Uri.parse('http://api.gimmillions.com/api/user/watchlist/add'),
        headers: <String, String>{
          'Content-Type': 'application/json; charset=UTF-8',
        },
        body: jsonEncode({
          'username': currentUser.username,
          'symbols': [r.symbol]
        }));

    if (response.statusCode != 200) {
      throw Exception('Unable to update user watchlist');
    }
  }

  Future<void> removeFromWatchlist(String symbol) async {
    this._watchlist.removeWhere((element) => element.symbol == symbol);
    final response = await http.put(Uri.parse('http://api.gimmillions.com/api/user/watchlist/remove'),
        headers: <String, String>{
          'Content-Type': 'application/json; charset=UTF-8',
        },
        body: jsonEncode({
          'username': currentUser.username,
          'symbols': [symbol]
        }));

    if (response.statusCode != 200) {
      throw Exception('Unable to update user watchlist');
    }
  }

  bool containsSymbol(String symbol) {
    return this._watchlist.any((element) => element == symbol);
  }

  Future<List<StockRecommendation>> refresh() async {
    _watchlist = [];
    if (!currentUser.isLoggedIn || currentUser.authdata == '') {
      return _watchlist;
    }

    final response = await http.get(
        Uri.parse('http://api.gimmillions.com/api/recommendations/stocks/user/${currentUser.username}'),
        headers: {'Authorization': 'Basic ${currentUser.authdata}'});

    if (response.statusCode == 200) {
      var json = jsonDecode(response.body) as List;
      json.forEach((element) {
        _watchlist.add(StockRecommendation.fromJson(element));
      });

      return watchlist;
    } else {
      throw Exception('Unable to retrieve user watchlist');
    }
  }
}
