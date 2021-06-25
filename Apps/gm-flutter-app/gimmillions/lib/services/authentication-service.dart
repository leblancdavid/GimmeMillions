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
    return await Future.delayed(Duration(milliseconds: 5000), () {
      _currentUser = User(0, 'mock', 'user', username, password, UserRole.SuperUser, '');
      _onAuthStateChangedController.add(_currentUser);
    });
  }

  Future<void> signOut() async {
    return await Future.delayed(Duration(milliseconds: 5000), () {
      _currentUser = null;
      _onAuthStateChangedController.add(_currentUser);
    });
  }
}
