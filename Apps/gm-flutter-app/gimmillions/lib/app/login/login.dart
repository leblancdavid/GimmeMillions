import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/models/user.dart';
import 'package:gimmillions/services/authentication-service.dart';
import 'package:provider/provider.dart';

class LoginWidget extends StatefulWidget {
  @override
  _LoginWidgetState createState() => _LoginWidgetState();
}

class _LoginWidgetState extends State<LoginWidget> {
  late TextEditingController _usernameController;
  late TextEditingController _passwordController;

  Future<User?> _login() async {
    try {
      if (_usernameController.text == '' || _passwordController.text == '') {
        return Future.error('Please specify a username and password');
      }
      print('${_usernameController.text}, ${_passwordController.text}');
      final service = Provider.of<AuthenticationService>(context, listen: false);
      return await service.signIn(_usernameController.text, _passwordController.text);
    } catch (e) {
      print(e);
    }
  }

  void initState() {
    super.initState();
    _usernameController = TextEditingController();
    _passwordController = TextEditingController();
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    var theme = Theme.of(context);
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        title: Image(
          image: AssetImage('assets/images/full-logo-light.png'),
          height: 24,
        ),
      ),
      body: SingleChildScrollView(
        child: Column(
          children: <Widget>[
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 60.0),
              child: Center(
                child: Container(
                    width: 200,
                    height: 150,
                    child: Image(
                      image: AssetImage('assets/images/logo.png'),
                      height: 24,
                    )),
              ),
            ),
            Padding(
              //padding: const EdgeInsets.only(left:15.0,right: 15.0,top:0,bottom: 0),
              padding: EdgeInsets.symmetric(horizontal: 15),
              child: TextField(
                controller: _usernameController,
                decoration: InputDecoration(
                    focusColor: theme.accentColor,
                    focusedBorder: OutlineInputBorder(borderSide: BorderSide(color: theme.accentColor)),
                    border: OutlineInputBorder(),
                    labelStyle: TextStyle(color: theme.primaryColor),
                    labelText: 'Username',
                    hintText: 'Enter valid username'),
              ),
            ),
            Padding(
              padding: const EdgeInsets.only(left: 15.0, right: 15.0, top: 15, bottom: 0),
              child: TextField(
                controller: _passwordController,
                obscureText: true,
                decoration: InputDecoration(
                    focusColor: theme.accentColor,
                    focusedBorder: OutlineInputBorder(borderSide: BorderSide(color: theme.accentColor)),
                    border: OutlineInputBorder(),
                    labelStyle: TextStyle(color: theme.primaryColor),
                    labelText: 'Password',
                    hintText: 'Enter secure password'),
              ),
            ),
            Padding(
                padding: EdgeInsets.symmetric(vertical: 32),
                child: Container(
                  height: 50,
                  width: 250,
                  child: ElevatedButton(
                    onPressed: () {
                      _login();
                      //Navigator.push(context, MaterialPageRoute(builder: (_) => HomePage()));
                    },
                    style: ElevatedButton.styleFrom(primary: theme.primaryColor),
                    child: Text(
                      'Login',
                      style: TextStyle(color: Colors.white, fontSize: 25),
                    ),
                  ),
                )),
            SizedBox(
              height: 130,
            ),
            Text('New User? Create Account')
          ],
        ),
      ),
    );
  }
}
