using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Authentication
{
    public static class UserExtensionMethods
    {
        public static IEnumerable<User> WithoutPasswords(this IEnumerable<User> users)
        {
            return users.Select(x => x.WithoutPassword());
        }

        public static User WithoutPassword(this User user)
        {
            user.PasswordHash = null;
            return user;
        }
    }
}
