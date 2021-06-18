import 'package:flutter/material.dart';

class StocksWidget extends StatefulWidget {
  @override
  State<StatefulWidget> createState() => _StocksWidgetState();
}

class _StocksWidgetState extends State<StocksWidget>
    with TickerProviderStateMixin {
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
        title: const Text('Gimmillions'),
        bottom: TabBar(
          controller: _tabController,
          tabs: const <Widget>[
            Tab(
              icon: Icon(Icons.stacked_line_chart),
            ),
            Tab(
              icon: Icon(Icons.online_prediction),
            ),
            Tab(
              icon: Icon(Icons.playlist_add_check),
            ),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: const <Widget>[
          Center(
            child: Text("It's cloudy here"),
          ),
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
