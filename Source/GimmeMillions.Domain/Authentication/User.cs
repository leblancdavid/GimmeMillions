namespace GimmeMillions.Domain.Authentication
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; private set; }

        public User()
        {
            Role = UserRole.Default;
        }

        public User(string firstName, string lastName, string userName, string password, UserRole role)
        {
            HashPassword(password);
            FirstName = firstName;
            LastName = lastName;
            Username = userName.ToLower();
            Role = role;
        }

        public void HashPassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        public static bool IsValidUsername(string username)
        {

            return true;
        }

        public static bool IsValidPassword(string password)
        {

            return true;
        }

    }
}
