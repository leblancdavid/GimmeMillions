import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/futures.dart';

class StocksWidget extends StatefulWidget {
  @override
  State<StatefulWidget> createState() => _StocksWidgetState();
}

class _StocksWidgetState extends State<StocksWidget> with TickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        flexibleSpace: TabBar(
          controller: _tabController,
          tabs: const <Widget>[
            Tab(
              icon: Icon(Icons.stacked_line_chart),
              text: 'Futures',
            ),
            Tab(
              icon: Icon(Icons.online_prediction),
              text: 'Predictions',
            ),
            Tab(
              icon: Icon(Icons.playlist_add_check),
              text: 'Watchlist',
            ),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: <Widget>[
          FuturesWidget(),
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
