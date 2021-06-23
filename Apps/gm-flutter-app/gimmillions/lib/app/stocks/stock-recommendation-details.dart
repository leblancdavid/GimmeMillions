import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

class StockRecommendationDetailsArguments {
  final String symbol;
  StockRecommendationDetailsArguments(this.symbol);
}

class StockRecommendationDetails extends StatelessWidget {
  static const routeName = '/stockDetails';

  @override
  Widget build(BuildContext context) {
    // Extract the arguments from the current ModalRoute
    // settings and cast them as ScreenArguments.
    final args = ModalRoute.of(context)!.settings.arguments as StockRecommendationDetailsArguments;

    return Scaffold(
      appBar: AppBar(
        title: Text(args.symbol),
      ),
      body: Center(
        child: Text('This is where the details will go'),
      ),
    );
  }
}
