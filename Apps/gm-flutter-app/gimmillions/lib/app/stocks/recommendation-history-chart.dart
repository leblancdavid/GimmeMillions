import 'package:fl_chart/fl_chart.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/models/stock-recommendation-history.dart';
import 'package:gimmillions/models/stock-recommendation.dart';

class RecommendationHistoryChart extends StatefulWidget {
  final StockRecommendationHistory history;

  RecommendationHistoryChart(this.history);

  @override
  State<StatefulWidget> createState() => _RecommendationHistoryChartState(history);
}

class _RecommendationHistoryChartState extends State<RecommendationHistoryChart> {
  StockRecommendationHistory history;

  late LineChartData chartData;

  String chartType = 'Sentiment';
  _RecommendationHistoryChartState(this.history);

  @override
  void initState() {
    super.initState();
  }

  LineChartData _getChartData() {
    const dateTextStyle = TextStyle(fontSize: 10, color: Colors.black, fontWeight: FontWeight.bold);
    var barData = LineChartBarData(
        spots:
            history.historicalData.map((e) => FlSpot(e.date.millisecondsSinceEpoch.toDouble(), e.sentiment)).toList(),
        colors: history.historicalData.map((e) => e.getRgb(25)).toList(),
        dotData: FlDotData(show: false),
        isCurved: true,
        belowBarData: BarAreaData(show: true, colors: history.historicalData.map((e) => e.getRgbo(25, 0.5)).toList()));
    return LineChartData(
      lineTouchData: LineTouchData(enabled: false),
      lineBarsData: [barData],
      minY: 0,
      maxY: 100,
      titlesData: FlTitlesData(
        bottomTitles: SideTitles(
            showTitles: true,
            reservedSize: 14,
            interval: 8.64e+7,
            getTextStyles: (value) => dateTextStyle,
            getTitles: (value) {
              var date = DateTime.fromMillisecondsSinceEpoch(value.toInt());
              return '${date.month}/${date.day}';
            }),
        leftTitles: SideTitles(
          showTitles: true,
          interval: 25,
          getTitles: (value) {
            return '${value}';
          },
        ),
      ),
      gridData: FlGridData(
        show: true,
        drawHorizontalLine: true,
        drawVerticalLine: true,
        horizontalInterval: 25,
        verticalInterval: 8.64e+7,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    chartData = _getChartData();
    return Column(mainAxisAlignment: MainAxisAlignment.center, children: <Widget>[
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
            chartData = _getChartData();
          });
        },
        items: <String>['Sentiment', 'Confidence', 'Price'].map<DropdownMenuItem<String>>((String value) {
          return DropdownMenuItem<String>(
            value: value,
            child: Text(value),
          );
        }).toList(),
      ),
      Expanded(
          child: Padding(
        padding: EdgeInsets.all(16),
        child: LineChart(chartData),
      ))
    ]);
  }
}
