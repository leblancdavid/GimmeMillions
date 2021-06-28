import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/stock-recommendation-data-table.dart';
import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/services/stock-recommendation-service.dart';
import 'package:material_floating_search_bar/material_floating_search_bar.dart';
import 'package:provider/provider.dart';

class PredictionsWidget extends StatefulWidget {
  @override
  _PredictionsState createState() => _PredictionsState();
}

class _PredictionsState extends State<PredictionsWidget> {
  late Future<List<StockRecommendation>> _predictionList;

  _refreshFutures(BuildContext context) {
    try {
      final service = Provider.of<StockRecommendationService>(context, listen: false);
      _predictionList = service.getDailyPicks();
      setState(() {});
    } catch (e) {
      print(e);
      return List.empty();
    }
  }

  @override
  Widget build(BuildContext context) {
    _refreshFutures(context);
    final isPortrait = MediaQuery.of(context).orientation == Orientation.portrait;
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
          StockRecommendationDataTableBuilder(_predictionList, _refreshFutures)
        ]));
  }
}
