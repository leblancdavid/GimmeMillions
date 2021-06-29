import 'dart:async';

import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/recommendation-history-chart.dart';
import 'package:gimmillions/models/stock-recommendation-history.dart';
import 'package:gimmillions/models/user-watchlist.dart';
import 'package:gimmillions/services/stock-recommendation-service.dart';
import 'package:provider/provider.dart';

class StockRecommendationDetailsArguments {
  final String symbol;
  StockRecommendationDetailsArguments(this.symbol);
}

class StockRecommendationDetails extends StatefulWidget {
  static const routeName = '/stockDetails';

  @override
  State<StatefulWidget> createState() => _StockRecommendationDetailsState();
}

class _StockRecommendationDetailsState extends State<StockRecommendationDetails> with TickerProviderStateMixin {
  late TabController _tabController;
  late UserWatchlist _watchlist;
  late Future<StockRecommendationHistory> history;
  late StockRecommendationDetailsArguments args;
  StreamController<String> watchStatus = StreamController<String>();

  @override
  void initState() {
    super.initState();
    _watchlist = Provider.of<UserWatchlist>(context, listen: false);
    _tabController = TabController(length: 2, vsync: this);
  }

  Future<StockRecommendationHistory> _getHistory(BuildContext context, String symbol) {
    try {
      final service = Provider.of<StockRecommendationService>(context, listen: false);
      return service.getHistoryFor(symbol);
    } catch (e) {
      print(e);
      return Future.error(e);
    }
  }

  @override
  Widget build(BuildContext context) {
    // Extract the arguments from the current ModalRoute
    // settings and cast them as ScreenArguments.

    args = ModalRoute.of(context)!.settings.arguments as StockRecommendationDetailsArguments;
    history = _getHistory(context, args.symbol);

    if (_watchlist.containsSymbol(args.symbol)) {
      watchStatus.add('Unwatch');
    } else {
      watchStatus.add('Watch');
    }

    return Scaffold(
      appBar: AppBar(
        title: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              args.symbol,
              style: TextStyle(fontSize: 20),
            ),
          ],
        ),
        bottom: TabBar(
          controller: _tabController,
          tabs: const <Widget>[
            Tab(
              icon: Icon(Icons.info),
              text: 'Info',
            ),
            Tab(
              icon: Icon(Icons.timeline),
              text: 'History',
            )
          ],
        ),
      ),
      body: Center(
        child: FutureBuilder(
            future: history,
            builder: (BuildContext context, AsyncSnapshot<StockRecommendationHistory> snapshot) {
              if (snapshot.connectionState != ConnectionState.done) {
                return CircularProgressIndicator(color: Theme.of(context).primaryColor);
              }

              if (snapshot.hasData) {
                const infoSpacing = 16.0;
                return TabBarView(
                  controller: _tabController,
                  children: <Widget>[
                    Container(
                      padding: EdgeInsets.all(16),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          SizedBox(height: infoSpacing),
                          Align(
                              alignment: Alignment.topRight,
                              child: StreamBuilder<String>(
                                  stream: watchStatus.stream,
                                  builder: (context, watchSnapshot) {
                                    return FloatingActionButton.extended(
                                        onPressed: () {
                                          if (watchSnapshot.hasData) {
                                            if (watchSnapshot.data == 'Watch') {
                                              _watchlist.addToWatchlist(snapshot.data!.lastRecommendation);
                                              watchStatus.add('Unwatch');
                                            } else {
                                              _watchlist.removeFromWatchlist(args.symbol);
                                              watchStatus.add('Watch');
                                            }
                                          }
                                        },
                                        backgroundColor: Theme.of(context).primaryColor,
                                        foregroundColor: Colors.white,
                                        icon: Icon(Icons.remove_red_eye_outlined),
                                        label: Text(watchSnapshot.hasData ? watchSnapshot.data! : ''));
                                  })),
                          SizedBox(height: infoSpacing),
                          RichText(
                              text: TextSpan(
                                  text: 'Recommendation: ',
                                  style: TextStyle(fontWeight: FontWeight.bold, color: Colors.black, fontSize: 20.0),
                                  children: <TextSpan>[
                                TextSpan(
                                    text: '${snapshot.data!.lastRecommendation.concensus}',
                                    style: TextStyle(
                                        fontWeight: FontWeight.bold,
                                        color: snapshot.data!.lastRecommendation.getRgb(25))),
                              ])),
                          SizedBox(height: infoSpacing),
                          RichText(
                              text: TextSpan(
                                  text: 'Sentiment: ',
                                  style: TextStyle(fontWeight: FontWeight.bold, color: Colors.black, fontSize: 20.0),
                                  children: <TextSpan>[
                                TextSpan(
                                    text: '${snapshot.data!.lastRecommendation.sentiment.toStringAsFixed(2)}%',
                                    style: TextStyle(
                                        fontWeight: FontWeight.bold,
                                        color: snapshot.data!.lastRecommendation.getRgb(25))),
                              ])),
                          SizedBox(height: infoSpacing),
                          RichText(
                              text: TextSpan(
                                  text: 'Confidence: ',
                                  style: TextStyle(fontWeight: FontWeight.bold, color: Colors.black, fontSize: 20.0),
                                  children: <TextSpan>[
                                TextSpan(
                                    text: '${snapshot.data!.lastRecommendation.confidence.toStringAsFixed(3)}',
                                    style: TextStyle(
                                        fontWeight: FontWeight.bold,
                                        color: snapshot.data!.lastRecommendation.confidence > 0
                                            ? Colors.green.shade800
                                            : Colors.red.shade800)),
                              ])),
                          SizedBox(height: infoSpacing),
                          RichText(
                              text: TextSpan(
                                  text: 'Last Close: ',
                                  style: TextStyle(fontWeight: FontWeight.bold, color: Colors.black, fontSize: 20.0),
                                  children: <TextSpan>[
                                TextSpan(
                                    text:
                                        '\$${snapshot.data!.lastRecommendation.stockData.previousClose.toStringAsFixed(2)} ',
                                    style: TextStyle(fontWeight: FontWeight.normal, color: Colors.black)),
                                TextSpan(
                                    text:
                                        '(${snapshot.data!.lastRecommendation.stockData.percentChangeFromPreviousClose.toStringAsFixed(2)}%)',
                                    style: TextStyle(
                                        fontWeight: FontWeight.bold,
                                        color:
                                            snapshot.data!.lastRecommendation.stockData.percentChangeFromPreviousClose >
                                                    0
                                                ? Colors.green.shade800
                                                : Colors.red.shade800)),
                              ])),
                          SizedBox(height: infoSpacing),
                          RichText(
                              text: TextSpan(
                                  text: 'High Prediction: ',
                                  style: TextStyle(fontWeight: FontWeight.bold, color: Colors.black, fontSize: 20.0),
                                  children: <TextSpan>[
                                TextSpan(
                                    text:
                                        '\$${snapshot.data!.lastRecommendation.predictedPriceTarget.toStringAsFixed(2)} ',
                                    style: TextStyle(fontWeight: FontWeight.normal, color: Colors.black)),
                                TextSpan(
                                    text: '(${snapshot.data!.lastRecommendation.prediction.toStringAsFixed(2)}%)',
                                    style: TextStyle(fontWeight: FontWeight.bold, color: Colors.green.shade800)),
                              ])),
                          SizedBox(height: infoSpacing),
                          RichText(
                              text: TextSpan(
                                  text: 'Low Prediction: ',
                                  style: TextStyle(fontWeight: FontWeight.bold, color: Colors.black, fontSize: 20.0),
                                  children: <TextSpan>[
                                TextSpan(
                                    text:
                                        '\$${snapshot.data!.lastRecommendation.predictedLowTarget.toStringAsFixed(2)} ',
                                    style: TextStyle(fontWeight: FontWeight.normal, color: Colors.black)),
                                TextSpan(
                                    text: '(${snapshot.data!.lastRecommendation.lowPrediction.toStringAsFixed(2)}%)',
                                    style: TextStyle(fontWeight: FontWeight.bold, color: Colors.red.shade800)),
                              ])),
                          SizedBox(height: infoSpacing),
                          Expanded(
                            child: Align(
                              alignment: Alignment.bottomCenter,
                              child: RichText(
                                  text: TextSpan(
                                text: 'Last Updated: ${snapshot.data!.lastUpdated.toIso8601String()}',
                                style: TextStyle(
                                    fontStyle: FontStyle.italic,
                                    fontWeight: FontWeight.normal,
                                    color: Colors.black,
                                    fontSize: 14.0),
                              )),
                            ),
                          ),
                        ],
                      ),
                    ),
                    RecommendationHistoryChart(snapshot.data!)
                  ],
                );
              }

              return CircularProgressIndicator(color: Theme.of(context).primaryColor);
            }),
      ),
    );
  }
}
