import 'dart:async';

import 'package:gimmillions/models/user.dart';

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

  Future<User?> signIn(String username, String password) async {
    _onAuthStateChangedController.add(null);
    return await Future.delayed(Duration(milliseconds: 1000), () {
      var newUser = User(0, 'mock', 'user', username, password, UserRole.SuperUser, '');
      newUser.isLoggedIn = true;
      _currentUser = newUser;
      _onAuthStateChangedController.add(_currentUser);
    });
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
