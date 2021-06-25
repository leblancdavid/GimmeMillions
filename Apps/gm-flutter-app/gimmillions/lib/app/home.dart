import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/futures.dart';
import 'package:gimmillions/app/stocks/stocks.dart';

class HomeWidget extends StatefulWidget {
  @override
  State<StatefulWidget> createState() => _HomeWidgetState();
}

class _HomeWidgetState extends State<HomeWidget> with TickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
  }

  @override
  Widget build(BuildContext context) {
    var theme = Theme.of(context);
    return Scaffold(
        appBar: AppBar(
          shadowColor: Colors.transparent,
          title: Row(
            children: [
              PopupMenuButton<String>(
                  icon: Icon(Icons.menu),
                  onSelected: (String result) {
                    if (result == 'test') {}
                  },
                  itemBuilder: (BuildContext context) => <PopupMenuEntry<String>>[
                        PopupMenuItem(
                            value: 'stocks',
                            child: Row(
                              children: [
                                Icon(
                                  Icons.show_chart,
                                  color: theme.primaryColor,
                                ),
                                Padding(
                                  padding: EdgeInsets.only(left: 16),
                                  child: Text("Stocks"),
                                )
                              ],
                            )),
                        PopupMenuDivider(),
                        PopupMenuItem(
                            value: 'profile',
                            child: Row(
                              children: [
                                Icon(
                                  Icons.person,
                                  color: theme.primaryColor,
                                ),
                                Padding(
                                  padding: EdgeInsets.only(left: 16),
                                  child: Text("Profile"),
                                )
                              ],
                            )),
                        PopupMenuDivider(),
                        PopupMenuItem(
                            value: 'logout',
                            child: Row(
                              children: [
                                Icon(
                                  Icons.login,
                                  color: theme.primaryColor,
                                ),
                                Padding(
                                  padding: EdgeInsets.only(left: 16),
                                  child: Text("Sign Out"),
                                )
                              ],
                            )),
                      ]),
              Image(
                image: AssetImage('assets/images/full-logo-light.png'),
                height: 24,
              )
            ],
          ),
        ),
        body: StocksWidget());
  }
}
