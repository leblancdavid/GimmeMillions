import 'dart:convert';

import 'package:flutter/cupertino.dart';
import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/models/user.dart';
import 'package:http/http.dart' as http;

class UserWatchlist extends ChangeNotifier {
  late List<StockRecommendation> watchlist;
  late User currentUser;

  UserWatchlist();

  Future<void> addToWatchlist(StockRecommendation r) async {
    this.watchlist.add(r);
    notifyListeners();

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
    this.watchlist.removeWhere((element) => element.symbol == symbol);
    notifyListeners();

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
    return this.watchlist.any((element) => element == symbol);
  }

  Future<List<StockRecommendation>> refresh() async {
    watchlist = [];
    if (currentUser == null || !currentUser.isLoggedIn || currentUser.authdata == '') {
      return watchlist;
    }

    final response = await http.get(
        Uri.parse('http://api.gimmillions.com/api/recommendations/stocks/user/${currentUser.username}'),
        headers: {'Authorization': 'Basic ${currentUser.authdata}'});

    if (response.statusCode == 200) {
      var json = jsonDecode(response.body) as List;
      json.forEach((element) {
        watchlist.add(StockRecommendation.fromJson(element));
      });
      notifyListeners();
      return watchlist;
    } else {
      throw Exception('Unable to retrieve user watchlist');
    }
  }
}
