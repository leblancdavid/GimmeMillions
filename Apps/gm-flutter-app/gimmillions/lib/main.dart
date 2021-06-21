// Copyright 2018 The Flutter team. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/stocks.dart';
import 'package:gimmillions/services/stock-recommendation-service.dart';
import 'package:provider/provider.dart';

void main() => runApp(MyApp());

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MultiProvider(
        providers: [
          Provider<StockRecommendationService>(
            create: (_) => StockRecommendationService(),
          ),
        ],
        child: MaterialApp(
          title: 'Startup Name Generator',
          theme: ThemeData(
              primaryColor: Color.fromRGBO(27, 96, 58, 1),
              accentColor: Colors.red),
          home: StocksWidget(),
        ));
  }
}
