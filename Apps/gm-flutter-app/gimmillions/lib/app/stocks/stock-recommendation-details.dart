import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

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

  @override
  Widget build(BuildContext context) {
    // Extract the arguments from the current ModalRoute
    // settings and cast them as ScreenArguments.
    final args = ModalRoute.of(context)!.settings.arguments as StockRecommendationDetailsArguments;

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
              text: 'Stats',
            ),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: <Widget>[
          Center(
            child: Text("It's rainy here"),
          ),
          Center(
            child: Text("It's sunny here"),
          ),
        ],
      ),
    );
  }
}
