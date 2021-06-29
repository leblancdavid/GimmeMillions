import 'package:fl_chart/fl_chart.dart';
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

  late LineChartData chartData;

  String chartType = 'Price';
  _RecommendationHistoryChartState(this.history);

  @override
  void initState() {
    super.initState();
  }

  LineChartData _getSentimentChartData() {
    const dateTextStyle = TextStyle(fontSize: 10, color: Colors.black, fontWeight: FontWeight.bold);

    var minY = history.historicalData
            .reduce((value, element) => value.sentiment < element.sentiment ? value : element)
            .sentiment *
        0.95;
    var maxY = history.historicalData
            .reduce((value, element) => value.sentiment > element.sentiment ? value : element)
            .sentiment *
        1.05;

    var intervalY = (maxY - minY).toInt() / 10;

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
      minY: minY,
      maxY: maxY,
      titlesData: FlTitlesData(
        bottomTitles: SideTitles(
            showTitles: true,
            reservedSize: 14,
            interval: 8.64e+7 * 5,
            getTextStyles: (value) => dateTextStyle,
            getTitles: (value) {
              var date = DateTime.fromMillisecondsSinceEpoch(value.toInt());
              return '${date.month}/${date.day}';
            }),
        leftTitles: SideTitles(
          showTitles: true,
          interval: intervalY,
          getTitles: (value) {
            return '${value.toStringAsFixed(0)}%';
          },
        ),
      ),
      gridData: FlGridData(
        show: true,
        drawHorizontalLine: true,
        drawVerticalLine: true,
        horizontalInterval: intervalY,
        verticalInterval: 8.64e+7,
      ),
    );
  }

  LineChartData _getPriceChartData() {
    const dateTextStyle = TextStyle(fontSize: 10, color: Colors.black, fontWeight: FontWeight.bold);
    var minY = history.historicalData
            .reduce((value, element) => value.stockData.close < element.stockData.close ? value : element)
            .stockData
            .close *
        0.99;
    var maxY = history.historicalData
            .reduce((value, element) => value.stockData.close > element.stockData.close ? value : element)
            .stockData
            .close *
        1.01;

    var intervalY = (maxY - minY).toInt() / 10;

    var barData = LineChartBarData(
        spots: history.historicalData
            .map((e) => FlSpot(e.date.millisecondsSinceEpoch.toDouble(), e.stockData.close))
            .toList(),
        colors: history.historicalData.map((e) => e.getRgb(25)).toList(),
        dotData: FlDotData(show: false),
        isCurved: true,
        belowBarData: BarAreaData(show: true, colors: history.historicalData.map((e) => e.getRgbo(25, 0.5)).toList()));
    return LineChartData(
      lineTouchData: LineTouchData(enabled: false),
      lineBarsData: [barData],
      minY: minY,
      maxY: maxY,
      titlesData: FlTitlesData(
        bottomTitles: SideTitles(
            showTitles: true,
            reservedSize: 14,
            interval: 8.64e+7 * 5,
            getTextStyles: (value) => dateTextStyle,
            getTitles: (value) {
              var date = DateTime.fromMillisecondsSinceEpoch(value.toInt());
              return '${date.month}/${date.day}';
            }),
        leftTitles: SideTitles(
          showTitles: true,
          reservedSize: 28,
          interval: intervalY,
          getTitles: (value) {
            return '\$${value.toStringAsFixed(0)}';
          },
        ),
      ),
      gridData: FlGridData(
        show: true,
        drawHorizontalLine: true,
        drawVerticalLine: true,
        horizontalInterval: intervalY,
        verticalInterval: 8.64e+7,
      ),
    );
  }

  LineChartData _getConfidenceChartData() {
    const dateTextStyle = TextStyle(fontSize: 10, color: Colors.black, fontWeight: FontWeight.bold);

    var minY = -1.0;
    var maxY = 1.0;
    var intervalY = (maxY - minY) / 10.0;

    var barData = LineChartBarData(
        spots:
            history.historicalData.map((e) => FlSpot(e.date.millisecondsSinceEpoch.toDouble(), e.confidence)).toList(),
        colors: history.historicalData.map((e) => e.getRgb(25)).toList(),
        dotData: FlDotData(show: false),
        isCurved: true,
        belowBarData: BarAreaData(show: true, colors: history.historicalData.map((e) => e.getRgbo(25, 0.5)).toList()));
    return LineChartData(
      lineTouchData: LineTouchData(enabled: false),
      lineBarsData: [barData],
      minY: minY,
      maxY: maxY,
      titlesData: FlTitlesData(
        bottomTitles: SideTitles(
            showTitles: true,
            reservedSize: 14,
            interval: 8.64e+7 * 5,
            getTextStyles: (value) => dateTextStyle,
            getTitles: (value) {
              var date = DateTime.fromMillisecondsSinceEpoch(value.toInt());
              return '${date.month}/${date.day}';
            }),
        leftTitles: SideTitles(
          showTitles: true,
          interval: intervalY,
          getTitles: (value) {
            return '${value.toStringAsFixed(2)}';
          },
        ),
      ),
      gridData: FlGridData(
        show: true,
        drawHorizontalLine: true,
        drawVerticalLine: true,
        horizontalInterval: intervalY,
        verticalInterval: 8.64e+7,
      ),
    );
  }

  LineChartData _getChartData() {
    if (chartType == 'Sentiment') {
      return _getSentimentChartData();
    } else if (chartType == 'Confidence') {
      return _getConfidenceChartData();
    }

    return _getPriceChartData();
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
        items: <String>['Price', 'Sentiment', 'Confidence'].map<DropdownMenuItem<String>>((String value) {
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
