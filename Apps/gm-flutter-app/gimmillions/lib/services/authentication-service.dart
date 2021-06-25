import 'package:gimmillions/models/user.dart';

class AuthenticationService {
  late User? _currentUser;

  Stream<User> _onAuthStateChanged = Stream.empty();
  Stream<User> get onAuthStateChanged {
    return _onAuthStateChanged;
  }

  bool get isUserAuthenticated {
    return _currentUser != null;
  }

  User? get currentUser {
    return _currentUser;
  }

  Future<User?> signIn(String username, String password) async {
    _currentUser = User(0, 'mock', 'user', username, password, UserRole.SuperUser, '');
    Future.delayed(Duration(milliseconds: 100), () => _currentUser);
  }

  Future<void> signOut() async {
    _currentUser = null;
    return Future.delayed(Duration(milliseconds: 100));
  }
}
