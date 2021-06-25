// Copyright 2018 The Flutter team. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/stock-recommendation-details.dart';
import 'package:gimmillions/app/stocks/stocks.dart';
import 'package:gimmillions/services/stock-recommendation-service.dart';
import 'package:provider/provider.dart';

void main() => runApp(MyApp());

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    var theme = ThemeData(primaryColor: Color.fromRGBO(27, 96, 58, 1), accentColor: Color.fromRGBO(196, 210, 83, 1));
    return MultiProvider(
        providers: [
          Provider<StockRecommendationService>(
            create: (_) => StockRecommendationService(),
          ),
        ],
        child: MaterialApp(
            routes: {
              StockRecommendationDetails.routeName: (context) => StockRecommendationDetails(),
            },
            title: 'Gimmillions',
            theme: theme,
            home: Scaffold(
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
                body: StocksWidget())));
  }
}
