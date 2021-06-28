enum UserRole { SuperUser, Administrator, Default }

class User {
  int id;
  String firstName;
  String lastName;
  String username;
  String password;
  UserRole role;
  String stocksWatchlistString;

  User(this.id, this.firstName, this.lastName, this.username, this.password, this.role, this.stocksWatchlistString);

  late bool isLoggedIn = false;

  factory User.fromJson(Map<String, dynamic> json) {
    return User(json['id'], json['firstName'], json['lastName'], json['username'], '', UserRole.values[json['role']],
        json['stocksWatchlistString']);
  }
}
