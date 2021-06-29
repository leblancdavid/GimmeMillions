import 'dart:async';
import 'dart:convert';

import 'package:gimmillions/models/user-watchlist.dart';
import 'package:gimmillions/models/user.dart';
import 'package:http/http.dart' as http;

class AuthenticationService {
  final UserWatchlist _watchlist;
  late User? _currentUser;
  Codec<String, String> _stringToBase64 = utf8.fuse(base64);

  AuthenticationService(this._watchlist) {}

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
    final response = await http.post(Uri.parse('http://api.gimmillions.com/api/user/authenticate'),
        headers: <String, String>{
          'Content-Type': 'application/json; charset=UTF-8',
        },
        body: jsonEncode({'username': username, 'password': password}));

    if (response.statusCode == 200) {
      _currentUser = User.fromJson(jsonDecode(response.body));
      _currentUser!.isLoggedIn = true;

      _currentUser!.authdata = _stringToBase64.encode(username + ':' + password);

      _watchlist.currentUser = _currentUser!;
      _watchlist.refresh();
      _onAuthStateChangedController.add(_currentUser);
      return _currentUser!;
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
