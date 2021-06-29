// Copyright 2018 The Flutter team. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import 'package:flutter/material.dart';
import 'package:gimmillions/app/login/authentication.dart';
import 'package:gimmillions/app/stocks/stock-recommendation-details.dart';
import 'package:gimmillions/models/user-watchlist.dart';
import 'package:gimmillions/services/authentication-service.dart';
import 'package:gimmillions/services/stock-recommendation-service.dart';
import 'package:provider/provider.dart';

void main() => runApp(MyApp());

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    var userWatchlist = UserWatchlist();
    var authenticationService = AuthenticationService(userWatchlist);
    var theme = ThemeData(primaryColor: Color.fromRGBO(27, 96, 58, 1), accentColor: Color.fromRGBO(196, 210, 83, 1));
    return MultiProvider(
        providers: [
          Provider<StockRecommendationService>(
            create: (_) => StockRecommendationService(authenticationService),
          ),
          Provider<AuthenticationService>(
            create: (_) => authenticationService,
          ),
          Provider<UserWatchlist>(
            create: (_) => userWatchlist,
          )
        ],
        child: AuthWidgetBuilder(builder: (context, userSnapshot) {
          return MaterialApp(routes: {
            StockRecommendationDetails.routeName: (context) => StockRecommendationDetails(),
          }, title: 'Gimmillions', theme: theme, home: AuthWidget(userSnapshot: userSnapshot));
        }));
  }
}
