import 'dart:async';

import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/stock-recommendation-data-table.dart';
import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/services/stock-recommendation-service.dart';
import 'package:provider/provider.dart';

class FuturesWidget extends StatefulWidget {
  @override
  _FuturesState createState() => _FuturesState();
}

class _FuturesState extends State<FuturesWidget> {
  late Future<List<StockRecommendation>> _futuresList;

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

    return Container(
        constraints: BoxConstraints.expand(),
        child: Column(mainAxisAlignment: MainAxisAlignment.start, children: <Widget>[
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
          StockRecommendationDataTableBuilder(_futuresList, _refreshFutures)
        ]));
  }
}
