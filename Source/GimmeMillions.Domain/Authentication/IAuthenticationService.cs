using CSharpFunctionalExtensions;

namespace GimmeMillions.Domain.Authentication
{
    public interface IAuthenticationService
    {
        Result<User> Authenticate(string username, string password);
        Result<User> AddOrUpdateUser(User user);
        Result<User> GetUser(string username);
        Result UpdatePassword(string username, string oldPassword, string newPassword);
        Result ResetPassword(string username, string newPassword);
        bool UserExists(string username);
        void RemoveUser(string username);

    }
}
