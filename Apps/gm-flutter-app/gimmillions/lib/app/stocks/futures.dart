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

enum _FuturesMenuOptions {
  refresh,
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

  late Future<List<StockRecommendation>> futuresList;

  @override
  Widget build(BuildContext context) {
    _refreshFutures(context);

    return Container(
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: <Widget>[
      Row(
        mainAxisAlignment: MainAxisAlignment.end,
        children: [
          PopupMenuButton<_FuturesMenuOptions>(
              onSelected: (_FuturesMenuOptions result) {
                if (result == _FuturesMenuOptions.refresh) {
                  _refreshFutures(context);
                }
              },
              itemBuilder: (BuildContext context) => <PopupMenuEntry<_FuturesMenuOptions>>[
                    PopupMenuItem(
                        value: _FuturesMenuOptions.refresh,
                        child: Row(
                          children: [Icon(Icons.refresh), Text("Refresh")],
                        ))
                  ]),
        ],
      ),
      Expanded(child: FuturesDataTableBuilder(_futuresList, _refreshFutures))
    ]));
  }
}

class FuturesDataTableBuilder extends StatelessWidget {
  final Future<List<StockRecommendation>> _futuresList;
  final Function onRefresh;

  const FuturesDataTableBuilder(this._futuresList, this.onRefresh);

  @override
  Widget build(BuildContext context) {
    return FutureBuilder(
        future: _futuresList,
        builder: (BuildContext context, AsyncSnapshot<List<StockRecommendation>> snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return Expanded(child: Center(child: CircularProgressIndicator(color: Theme.of(context).primaryColor)));
          }

          if (snapshot.hasData) {
            return StockRecommendationDataTable(snapshot.data!);
          }

          return Expanded(child: Center(child: CircularProgressIndicator(color: Theme.of(context).primaryColor)));
        });
  }

  List<DataRow> _toTableRows(List<StockRecommendation>? futures) {
    if (futures == null) {
      return List.empty();
    }

    return futures
        .map((e) => DataRow(cells: [
              DataCell(Text(e.symbol)),
              DataCell(Text(e.sentiment.toStringAsFixed(2))),
              DataCell(Text(e.confidence.toStringAsFixed(3))),
            ]))
        .toList();
  }
}
