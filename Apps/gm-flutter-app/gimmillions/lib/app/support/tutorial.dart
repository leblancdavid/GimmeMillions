import 'dart:async';

import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

class TutorialWidget extends StatefulWidget {
  static const routeName = '/tutorial';

  @override
  State<StatefulWidget> createState() => _TutorialState();
}

class _TutorialState extends State<TutorialWidget> {
  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    var theme = Theme.of(context);

    var gimmillionsRef = TextSpan(
        text: 'Gimmillions',
        style: TextStyle(color: theme.primaryColor, fontWeight: FontWeight.bold, fontStyle: FontStyle.italic));
    var disclaimerRef = TextSpan(
        text: 'disclaimer',
        style: TextStyle(color: theme.accentColor, fontWeight: FontWeight.bold, decoration: TextDecoration.underline));
    return Scaffold(
      appBar: AppBar(
        title: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              'How to Use',
              style: TextStyle(fontSize: 20),
            ),
          ],
        ),
      ),
      body: ListView(
        padding: EdgeInsets.all(8),
        children: [
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(text: 'Before using '),
                    gimmillionsRef,
                    TextSpan(text: ', please make sure you read through the '),
                    disclaimerRef,
                    TextSpan(text: '.'),
                  ]))),
          Container(
              padding: EdgeInsets.fromLTRB(0, 24, 0, 0),
              child: Text('Algorithm Overview',
                  style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold, decoration: TextDecoration.underline))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(text: 'The '),
                    gimmillionsRef,
                    TextSpan(
                        text:
                            '''algorithm uses machine learning and artificial intelligence to generate predictions daily predictions on many stock symbols. The machine learning model has been trained using years of historical data. Various indicators are computed on the price and volume of each stock, which is then used to compute the predictions.'''),
                  ]))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(
                        text:
                            'The algorithm makes three different predictions: a buy/sell signal, a projected gain with target, and a projected loss. As discussed in the '),
                    disclaimerRef,
                    TextSpan(
                        text:
                            ''', the accuracy of these targets can vary based on many factors, therefore they should not necessary be taken as absolute truth. They are meant to help in the technical analysis process when making investment decisions. These predictions should always be supported by your own investigative diligence. For more details, see below.'''),
                  ]))),
          Container(
              padding: EdgeInsets.fromLTRB(0, 24, 0, 0),
              child: Text('Understanding Predictions',
                  style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold, decoration: TextDecoration.underline))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(
                        text:
                            'The predictions are updated on a daily basis. The algorithm\'s main prediction is the signal, which is a sentiment of strength for either buying or selling. A signal of 100% represents a buy signal while a signal of 0% would represent a sell signal (in practice, the current algorithm occasionally produces signals which are larger than 100% or less than 0%, these should be treated as buy and sell signals respectively).'),
                  ]))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(
                        text:
                            'Signals which are greater than 90% are typically considered strong buy signals and inversely signals less than 10% are categorized as strong sell. Signals between 75% and 90% and those between 10% and 25% are categorized as buy and sell signals respectively. The remaining signals are labelled simply as hold. Filters found in the menu allow the user to easily filter predictions by signal category. Note: strong buy or sell signals should not be blindly followed, they are simply values predicted by an algorithm. Always do your own due diligence before making a decision on investments.'),
                  ]))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(
                        text:
                            'The algorithm makes daily predictions for possible swing trading, not necessarily for day trading. The time to hold a security can range from days to months, depending on the price action of the stock, and the trading strategy of each individual.'),
                  ]))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(
                        text:
                            'Along with the signal, the algorithm also attempts to predict possible high and low price targets. It is important to remember that these predictions are only estimates and should be treated as such. They are meant to be a tool to aid an investor in visualizing potential gains and risks. The accuracy of these predictions is greatly affected by market volatility, news, earnings reports, and many other factors. It is not unusual for the algorithm to be confused by a certain unknown or abnormal pattern in a stocks price action. Occasionally, the predicted targets may be nonsensical, such as abnormally large gains or losses, or a high target which is lower than the low target. Again, these predictions are merely a tool. Predicted should be backed up with careful study of the stock\'s chart.'),
                  ]))),
          Container(
              padding: EdgeInsets.fromLTRB(0, 24, 0, 0),
              child: Text('Tools',
                  style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold, decoration: TextDecoration.underline))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(text: 'The '),
                    gimmillionsRef,
                    TextSpan(
                        text:
                            ' app provides tools for users such as exporting predictions to files based on search criteria. Symbols can be looked up using the search bar. The algorithm tracks thousands of commonly traded symbols. If a symbol is not found in the list of predictions, a user may perform a search on a specific symbol. '),
                  ]))),
          Container(
              padding: EdgeInsets.fromLTRB(0, 24, 0, 0),
              child: Text('More features will be added in the future so stay tuned!',
                  textAlign: TextAlign.center,
                  style: TextStyle(fontSize: 12, fontWeight: FontWeight.normal, fontStyle: FontStyle.italic))),
        ],
      ),
    );
  }
}
