import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/stock-recommendation-data-table.dart';
import 'package:gimmillions/models/stock-recommendation-filter.dart';
import 'package:gimmillions/models/stock-recommendation.dart';
import 'package:gimmillions/services/stock-recommendation-service.dart';
import 'package:provider/provider.dart';

class PredictionsWidget extends StatefulWidget {
  @override
  _PredictionsState createState() => _PredictionsState();
}

class _PredictionsState extends State<PredictionsWidget> {
  late Future<List<StockRecommendation>> _predictionList;
  late TextEditingController _searchController;
  final StockRecommendationFilter _filter = StockRecommendationFilter();

  void initState() {
    super.initState();
    _searchController = TextEditingController();
    setState(() {});
  }

  _refreshPredictions(BuildContext context) {
    try {
      final service = Provider.of<StockRecommendationService>(context, listen: false);
      _predictionList = service.getDailyPicks();
      setState(() {});
    } catch (e) {
      print(e);
      return List.empty();
    }
  }

  _applySearchFilter(String value) {
    _filter.symbolFilter = value;
  }

  @override
  Widget build(BuildContext context) {
    _refreshPredictions(context);
    var theme = Theme.of(context);
    return Padding(
        padding: EdgeInsets.only(bottom: 32),
        child: ListView(children: <Widget>[
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
                  _refreshPredictions(context);
                },
                icon: Icon(
                  Icons.refresh,
                  color: Theme.of(context).primaryColor,
                ))
          ]),
          StockRecommendationDataTableBuilder(_predictionList, _filter)
        ]));
  }
}
