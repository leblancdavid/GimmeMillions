import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/recommendation-history-chart.dart';
import 'package:gimmillions/models/stock-recommendation-history.dart';
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

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
  }

  _getHistory(BuildContext context, String symbol) {
    try {
      final service = Provider.of<StockRecommendationService>(context, listen: false);
      return service.getHistoryFor(symbol);
    } catch (e) {
      print(e);
      return List.empty();
    }
  }

  @override
  Widget build(BuildContext context) {
    // Extract the arguments from the current ModalRoute
    // settings and cast them as ScreenArguments.
    final args = ModalRoute.of(context)!.settings.arguments as StockRecommendationDetailsArguments;
    var history = _getHistory(context, args.symbol);
    return Scaffold(
      appBar: AppBar(
        title: Text(args.symbol),
        bottom: TabBar(
          controller: _tabController,
          tabs: const <Widget>[
            Tab(
              icon: Icon(Icons.timeline),
              text: 'History',
            ),
            Tab(
              icon: Icon(Icons.info),
              text: 'Info',
            ),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: <Widget>[
          Center(
            child: FutureBuilder(
                future: history,
                builder: (BuildContext context, AsyncSnapshot<StockRecommendationHistory> snapshot) {
                  if (snapshot.connectionState != ConnectionState.done) {
                    return CircularProgressIndicator(color: Theme.of(context).primaryColor);
                  }

                  if (snapshot.hasData) {
                    return RecommendationHistoryChart(snapshot.data!);
                  }

                  return CircularProgressIndicator(color: Theme.of(context).primaryColor);
                }),
          ),
          Center(
            child: Text("It's sunny here"),
          ),
        ],
      ),
    );
  }
}
