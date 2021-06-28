import 'dart:async';
import 'dart:convert';

import 'package:gimmillions/models/user.dart';
import 'package:http/http.dart' as http;

class AuthenticationService {
  late User? _currentUser;

  StreamController<User?> _onAuthStateChangedController = StreamController<User?>();
  Stream<User?> get onAuthStateChanged {
    return _onAuthStateChangedController.stream;
  }

  bool get isUserAuthenticated {
    return _currentUser != null;
  }

  User? get currentUser {
    return _currentUser;
  }

  Future<User> signIn(String username, String password) async {
    print('Requesting sign in');
    final response = await http.post(Uri.parse('http://api.gimmillions.com/api/user/authenticate'),
        headers: <String, String>{
          'Content-Type': 'application/json; charset=UTF-8',
        },
        body: jsonEncode({'username': username, 'password': password}));

    print(response.statusCode);
    if (response.statusCode == 200) {
      return User.fromJson(jsonDecode(response.body));
    } else {
      throw Exception('Invalid username or password');
    }
  }

  Future<void> signOut() async {
    _currentUser = null;
    _onAuthStateChangedController.add(null);
    return await Future.delayed(Duration(milliseconds: 1000), () {
      var loggedOutUser = User(0, '', '', '', '', UserRole.Default, '');
      loggedOutUser.isLoggedIn = false;
      _currentUser = loggedOutUser;
      _onAuthStateChangedController.add(_currentUser);
    });
  }
}
