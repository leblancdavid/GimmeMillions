import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/stock-recommendation-data-table.dart';
import 'package:gimmillions/models/stock-recommendation-filter.dart';
import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/models/user-watchlist.dart';
import 'package:provider/provider.dart';

class UserWatchlistViewWidget extends StatefulWidget {
  @override
  _UserWatchlistViewState createState() => _UserWatchlistViewState();
}

class _UserWatchlistViewState extends State<UserWatchlistViewWidget> {
  late UserWatchlist _watchlist;
  late Future<List<StockRecommendation>> _watchlistFuture;
  late TextEditingController _searchController;
  final StockRecommendationFilter _filter = StockRecommendationFilter();

  void initState() {
    super.initState();
    _searchController = TextEditingController();
    _watchlist = Provider.of<UserWatchlist>(context, listen: false);
    _watchlistFuture = _watchlist.watchlist;
    _refreshWatchlist(context);
    setState(() {});
  }

  Future<void> _refreshWatchlist(BuildContext context) async {
    try {
      _watchlistFuture = _watchlist.refresh();
      setState(() {});
    } catch (e) {
      print(e);
    }
  }

  _applySearchFilter(String value) {
    _filter.symbolFilter = value;
  }

  @override
  Widget build(BuildContext context) {
    var theme = Theme.of(context);
    return ListView(children: <Widget>[
      Row(mainAxisAlignment: MainAxisAlignment.end, children: [
        Expanded(
            child: Padding(
                padding: EdgeInsets.all(8),
                child: TextField(
                  onChanged: (String value) {
                    _applySearchFilter(value);
                  },
                  cursorColor: theme.primaryColor,
                  style: TextStyle(),
                  controller: _searchController,
                  decoration: InputDecoration(
                      icon: Icon(Icons.search, color: theme.primaryColor),
                      focusColor: theme.accentColor,
                      focusedBorder: OutlineInputBorder(borderSide: BorderSide(color: theme.accentColor)),
                      border: OutlineInputBorder(),
                      labelStyle: TextStyle(color: theme.primaryColor),
                      hintText: 'Search...'),
                ))),
        IconButton(
            onPressed: () {
              _refreshWatchlist(context);
            },
            icon: Icon(
              Icons.refresh,
              color: Theme.of(context).primaryColor,
            ))
      ]),
      StockRecommendationDataTableBuilder(_watchlistFuture, _filter)
    ]);
  }
}
