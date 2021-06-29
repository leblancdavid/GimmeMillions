import 'dart:async';

import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/stock-recommendation-data-table.dart';
import 'package:gimmillions/models/stock-recommendation-filter.dart';
import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/services/stock-recommendation-service.dart';
import 'package:provider/provider.dart';

class FuturesWidget extends StatefulWidget {
  @override
  _FuturesState createState() => _FuturesState();
}

class _FuturesState extends State<FuturesWidget> {
  late Future<List<StockRecommendation>> _futuresList;
  final StockRecommendationFilter _filter = StockRecommendationFilter();

  _refreshFutures(BuildContext context) {
    try {
      final service = Provider.of<StockRecommendationService>(context, listen: false);
      _futuresList = service.getFutures();
      setState(() {});
    } catch (e) {
      print(e);
      return List.empty();
    }
  }

  @override
  Widget build(BuildContext context) {
    _refreshFutures(context);

    return Padding(
        padding: EdgeInsets.only(bottom: 32),
        child: Expanded(
            child: ListView(children: <Widget>[
          Row(mainAxisAlignment: MainAxisAlignment.end, children: [
            IconButton(
                onPressed: () {
                  _refreshFutures(context);
                },
                icon: Icon(
                  Icons.refresh,
                  color: Theme.of(context).primaryColor,
                ))
          ]),
          StockRecommendationDataTableBuilder(_futuresList, _filter)
        ])));
  }
}
