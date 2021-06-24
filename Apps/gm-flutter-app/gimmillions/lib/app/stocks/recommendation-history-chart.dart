import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/models/stock-recommendation-history.dart';

class RecommendationHistoryChart extends StatefulWidget {
  final StockRecommendationHistory history;

  RecommendationHistoryChart(this.history);

  @override
  State<StatefulWidget> createState() => _RecommendationHistoryChartState(history);
}

class _RecommendationHistoryChartState extends State<RecommendationHistoryChart> {
  StockRecommendationHistory history;
  String chartType = 'Sentiment';
  _RecommendationHistoryChartState(this.history);

  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: <Widget>[
      DropdownButton<String>(
        value: chartType,
        icon: const Icon(Icons.arrow_downward),
        iconSize: 24,
        elevation: 16,
        style: TextStyle(color: Theme.of(context).primaryColor),
        underline: Container(
          height: 2,
          color: Theme.of(context).primaryColor,
        ),
        onChanged: (String? newValue) {
          setState(() {
            chartType = newValue!;
          });
        },
        items: <String>['Sentiment', 'Confidence', 'Price'].map<DropdownMenuItem<String>>((String value) {
          return DropdownMenuItem<String>(
            value: value,
            child: Text(value),
          );
        }).toList(),
      )
    ]));
  }
}
