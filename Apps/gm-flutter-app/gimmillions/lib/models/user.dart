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
}
