import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/app/home.dart';
import 'package:gimmillions/models/user.dart';
import 'package:gimmillions/services/authentication-service.dart';
import 'package:provider/provider.dart';

import 'login.dart';

class AuthWidgetBuilder extends StatelessWidget {
  const AuthWidgetBuilder({Key? key, required this.builder}) : super(key: key);
  final Widget Function(BuildContext, AsyncSnapshot<User?>) builder;

  @override
  Widget build(BuildContext context) {
    final authService = Provider.of<AuthenticationService>(context, listen: false);
    return StreamBuilder<User?>(
      stream: authService.onAuthStateChanged,
      builder: (context, snapshot) {
        var user = snapshot.data;
        if (user != null) {
          return MultiProvider(
            providers: [
              Provider<User>.value(value: user),
            ],
            child: builder(context, snapshot),
          );
        }
        return builder(context, snapshot);
      },
    );
  }
}

class AuthWidget extends StatelessWidget {
  final AsyncSnapshot<User?> userSnapshot;

  const AuthWidget({Key? key, required this.userSnapshot}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    print(userSnapshot);
    if (userSnapshot.connectionState == ConnectionState.active) {
      if (!userSnapshot.hasData) {
        return LoginWidget();
      }

      print(userSnapshot);
      return userSnapshot.hasData && userSnapshot.data != null ? HomeWidget() : LoginWidget();
    }
    return Scaffold(
      body: Center(
        child: CircularProgressIndicator(),
      ),
    );
  }
}
