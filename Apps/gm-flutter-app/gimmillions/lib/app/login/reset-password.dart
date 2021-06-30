import 'dart:async';

import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/models/stock-recommendation-history.dart';
import 'package:gimmillions/models/user-watchlist.dart';
import 'package:gimmillions/services/authentication-service.dart';
import 'package:gimmillions/services/stock-recommendation-service.dart';
import 'package:provider/provider.dart';

class ResetPasswordWidget extends StatefulWidget {
  static const routeName = '/resetPassword';

  @override
  State<StatefulWidget> createState() => _ResetPasswordWidgetState();
}

class _ResetPasswordWidgetState extends State<ResetPasswordWidget> {
  late TextEditingController _oldPasswordController;
  late TextEditingController _newPasswordController;
  late TextEditingController _newPasswordConfirmController;
  late Future<void>? resetStatus = null;

  @override
  void initState() {
    super.initState();
    _oldPasswordController = TextEditingController();
    _newPasswordController = TextEditingController();
    _newPasswordConfirmController = TextEditingController();
  }

  Future<void> _resetPassword(BuildContext context) {
    try {
      final service = Provider.of<AuthenticationService>(context, listen: false);

      if (_oldPasswordController.text == _newPasswordConfirmController.text) {
        return Future.error('New password must be different than the old one');
      }
      if (_newPasswordConfirmController.text != _newPasswordController.text) {
        return Future.error('Confirm password does not match');
      }
      return service.resetPassword(_oldPasswordController.text, _newPasswordConfirmController.text);
    } catch (e) {
      print(e);
      return Future.error(e);
    }
  }

  @override
  Widget build(BuildContext context) {
    var theme = Theme.of(context);
    return Scaffold(
      appBar: AppBar(
        title: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              'Reset Password',
              style: TextStyle(fontSize: 20),
            ),
          ],
        ),
      ),
      body: Center(
        child: Column(children: <Widget>[
          Padding(
            //padding: const EdgeInsets.only(left:15.0,right: 15.0,top:0,bottom: 0),
            padding: EdgeInsets.fromLTRB(16, 32, 16, 16),
            child: TextField(
              cursorColor: theme.primaryColor,
              controller: _oldPasswordController,
              obscureText: true,
              decoration: InputDecoration(
                  focusColor: theme.accentColor,
                  focusedBorder: OutlineInputBorder(borderSide: BorderSide(color: theme.accentColor)),
                  border: OutlineInputBorder(),
                  labelStyle: TextStyle(color: theme.primaryColor),
                  labelText: 'Old Password',
                  hintText: 'Enter your previous password'),
            ),
          ),
          Padding(
            //padding: const EdgeInsets.only(left:15.0,right: 15.0,top:0,bottom: 0),
            padding: EdgeInsets.fromLTRB(16, 0, 16, 16),
            child: TextField(
              cursorColor: theme.primaryColor,
              controller: _newPasswordController,
              obscureText: true,
              decoration: InputDecoration(
                  focusColor: theme.accentColor,
                  focusedBorder: OutlineInputBorder(borderSide: BorderSide(color: theme.accentColor)),
                  border: OutlineInputBorder(),
                  labelStyle: TextStyle(color: theme.primaryColor),
                  labelText: 'New Password',
                  hintText: 'Enter your new password'),
            ),
          ),
          Padding(
            //padding: const EdgeInsets.only(left:15.0,right: 15.0,top:0,bottom: 0),
            padding: EdgeInsets.fromLTRB(16, 0, 16, 16),
            child: TextField(
              cursorColor: theme.primaryColor,
              controller: _newPasswordConfirmController,
              obscureText: true,
              decoration: InputDecoration(
                  focusColor: theme.accentColor,
                  focusedBorder: OutlineInputBorder(borderSide: BorderSide(color: theme.accentColor)),
                  border: OutlineInputBorder(),
                  labelStyle: TextStyle(color: theme.primaryColor),
                  labelText: 'Confirm New Password',
                  hintText: 'Confirm your new password'),
            ),
          ),
          Padding(
              padding: EdgeInsets.symmetric(vertical: 32),
              child: FutureBuilder(
                  future: resetStatus,
                  builder: (BuildContext context, AsyncSnapshot<void> snapshot) {
                    print(snapshot);
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return CircularProgressIndicator(color: Theme.of(context).primaryColor);
                    }

                    var loginButton = Container(
                      height: 50,
                      width: 250,
                      child: ElevatedButton(
                        onPressed: () {
                          setState(() {
                            resetStatus = _resetPassword(context);
                          });
                        },
                        style: ElevatedButton.styleFrom(primary: theme.primaryColor),
                        child: Text(
                          'Reset',
                          style: TextStyle(color: Colors.white, fontSize: 25),
                        ),
                      ),
                    );

                    if (snapshot.hasError) {
                      return Column(children: [
                        loginButton,
                        Padding(
                            padding: EdgeInsets.all(16),
                            child: Text('${snapshot.error}', style: TextStyle(color: theme.errorColor))),
                      ]);
                    }

                    if (snapshot.connectionState == ConnectionState.done) {
                      return Column(children: [
                        loginButton,
                        Padding(
                            padding: EdgeInsets.all(16),
                            child: Text('Successfully reset', style: TextStyle(color: theme.primaryColor))),
                      ]);
                    }

                    return loginButton;
                  })),
        ]),
      ),
    );
  }
}

class HomePage {}
