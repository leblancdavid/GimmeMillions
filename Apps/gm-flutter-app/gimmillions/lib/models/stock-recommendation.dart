import 'package:flutter/cupertino.dart';
import 'package:gimmillions/models/stock-data.dart';

class StockRecommendation {
  DateTime date;
  String symbol;
  String systemId;
  double sentiment;
  double confidence;
  double prediction;
  double lowPrediction;
  double previousClose;
  double predictedPriceTarget;
  double predictedLowTarget;
  StockData stockData;

  StockRecommendation(this.date, this.symbol, this.systemId, this.sentiment, this.confidence, this.prediction,
      this.lowPrediction, this.previousClose, this.predictedPriceTarget, this.predictedLowTarget, this.stockData);

  String get concensus {
    if (sentiment > 90) {
      return 'STRONG BUY';
    } else if (sentiment >= 75) {
      return "BUY";
    } else if (sentiment > 25) {
      return "HOLD";
    } else if (sentiment > 10) {
      return "SELL";
    } else {
      return "STRONG SELL";
    }
  }

  Color getRgb(int intensity) {
    var range = 255 - intensity;
    var b = intensity.toDouble();
    var g = 0.0;
    var r = 0.0;
    if (this.sentiment > 50.0) {
      r = intensity.toDouble();
      g = ((this.sentiment - 50.0) / 50.0) * range + intensity;
    } else {
      g = intensity.toDouble();
      r = ((50.0 - this.sentiment) / 50.0) * range + intensity;
    }
    return Color.fromRGBO(r.toInt(), g.toInt(), b.toInt(), 1);
  }

  Color getRgbo(int intensity, double opacity) {
    var range = 255 - intensity;
    var b = intensity.toDouble();
    var g = 0.0;
    var r = 0.0;
    if (this.sentiment > 50.0) {
      r = intensity.toDouble();
      g = ((this.sentiment - 50.0) / 50.0) * range + intensity;
    } else {
      g = intensity.toDouble();
      r = ((50.0 - this.sentiment) / 50.0) * range + intensity;
    }
    return Color.fromRGBO(r.toInt(), g.toInt(), b.toInt(), opacity);
  }

  factory StockRecommendation.fromJson(Map<String, dynamic> json) {
    return StockRecommendation(
        DateTime.parse(json['date']),
        json['symbol'],
        json['systemId'],
        json['sentiment'].toDouble(),
        json['confidence'].toDouble(),
        json['prediction'].toDouble(),
        json['lowPrediction'].toDouble(),
        json['previousClose'].toDouble(),
        json['predictedPriceTarget'].toDouble(),
        json['predictedLowTarget'].toDouble(),
        StockData.fromJson(json['lastData']));
  }
}
