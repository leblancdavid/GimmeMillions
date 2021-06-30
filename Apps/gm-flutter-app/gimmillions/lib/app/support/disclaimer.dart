import 'package:flutter/cupertino.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/app/support/tutorial.dart';

class DisclaimerWidget extends StatefulWidget {
  static const routeName = '/disclaimer';

  @override
  State<StatefulWidget> createState() => _DisclaimerState();
}

class _DisclaimerState extends State<DisclaimerWidget> {
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
    var howToUseRef = TextSpan(
      text: 'How to Use',
      style: TextStyle(color: theme.accentColor, fontWeight: FontWeight.bold, decoration: TextDecoration.underline),
      recognizer: new TapGestureRecognizer()..onTap = () => Navigator.pushNamed(context, TutorialWidget.routeName),
    );
    return Scaffold(
      appBar: AppBar(
        title: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              'Disclaimer',
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
                    TextSpan(text: ', please make sure you read through the following important information:'),
                  ]))),
          Container(
              padding: EdgeInsets.fromLTRB(0, 24, 0, 0),
              child: Text('Do your Own Due Diligence',
                  style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold, decoration: TextDecoration.underline))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(text: 'The predictions and content provided by the '),
                    gimmillionsRef,
                    TextSpan(
                        text:
                            ''' application is intented to be used as an informational tool to help investors find opportunities to invest in. It is crucial that you do your own analysis and research before making any investment decisions, based on your own investment strategies. Financial advice should be obtained from a certified professional or from your own independent research, not solely based on information provided by '''),
                    gimmillionsRef,
                    TextSpan(text: '.'),
                  ]))),
          Container(
              padding: EdgeInsets.fromLTRB(0, 24, 0, 0),
              child: Text('No Investment Advice',
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
                            ' application provides predictions based on public sources of data. We are not a broker/dealer or investment advisor, and we have no access to non-public information about publicly traded companies. We are not a source for financial advice, whether for investment decisions or tax or legal advice. We are not regulated by the Financial Services Authority.'),
                  ]))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(
                        text:
                            'The information provided in this application is meant to be an educational tool for learning and research. No content on the application constitutes - or should be understood as constituting - a recommendation to enter in any securities transactions or to engage in any of the investment strategies presented in our application content. We are not liable for any damages, expenses or other losses you may incur from using the information provided in this application.'),
                  ]))),
          Container(
              padding: EdgeInsets.fromLTRB(0, 24, 0, 0),
              child: Text('Additional Notes',
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
                            ' application uses historical data, technical analysis, and machine learning to make predictions and provide information regarding various securities. The data presented is obtained from public sources and could be incorrect, incomplete and/or inaccurate. Therefore, it is important to do you own research to validate that the information presented is in fact accurate. Because the predictions are based on this data and uses technical analysis and machine learning, the predictions should never be assumed to be absolutely accurate. The price of securities fluctuates based on many different factors such as news, overall markets, fundamental data, and various other unknown sources of volatility.'),
                  ]))),
          Container(
              padding: EdgeInsets.all(8),
              child: RichText(
                  textAlign: TextAlign.justify,
                  text: TextSpan(style: TextStyle(color: Colors.black, fontSize: 16), children: [
                    TextSpan(
                        text:
                            'The predictions made by the model should not be relied on as truthful or accurate, but more as an indication of sentiment or estimate of the possible outcome for a given security. The gains or losses for an individual security may vary or stray from the predictions. The predictions can be used as an informational tool. However, ultimately the responsibility of defining an investment plan/strategy and managing risk falls solely on the investor. See the '),
                    howToUseRef,
                    TextSpan(text: ' page for additional details.'),
                  ]))),
        ],
      ),
    );
  }
}
