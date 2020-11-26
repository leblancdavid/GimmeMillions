using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Authentication;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GimmeMillions.Database
{
    public class SQLUserRepository : IUserService
    {
        private readonly DbContextOptions<GimmeMillionsContext> _dbContextOptions;
        private object _saveLock = new object();
        public SQLUserRepository(DbContextOptions<GimmeMillionsContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }
        public Result<User> AddOrUpdateUser(User user)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);

                var existingUser = context.Users.FirstOrDefault(x => x.Username == user.Username);
                if (existingUser == null)
                {
                    context.Users.Add(user);
                    
                }
                existingUser.PasswordHash = user.PasswordHash;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;

                lock (_saveLock)
                {
                    context.SaveChanges();
                }
                return Result.Success(user);

            }
            catch (Exception ex)
            {
                return Result.Failure<User>(ex.Message);
            }
        }

        public Result<User> Authenticate(string username, string password)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            var existingUser = context.Users.FirstOrDefault(x => x.Username == username);
            if (existingUser == null || !existingUser.VerifyPassword(password)) 
                return Result.Failure<User>("Invalid username or password");

            return Result.Success(existingUser);
        }

        public Result<User> GetUser(string username)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            var existingUser = context.Users.FirstOrDefault(x => x.Username == username);
            if (existingUser == null)
                return Result.Failure<User>("Invalid username");

            return Result.Success(existingUser);
        }

        public void RemoveUser(string username)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);
                var existingUser = context.Users.FirstOrDefault(x => x.Username == username);
                if (existingUser != null)
                {
                    context.Remove(existingUser);
                    lock (_saveLock)
                    {
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception) { }
        }

        public Result ResetPassword(string username, string newPassword)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);

                var existingUser = context.Users.FirstOrDefault(x => x.Username == username);
                if (existingUser == null)
                {
                    return Result.Failure("Invalid username");

                }
                existingUser.HashPassword(newPassword);

                lock (_saveLock)
                {
                    context.SaveChanges();
                }
                return Result.Success();

            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public Result UpdatePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);

                var existingUser = context.Users.FirstOrDefault(x => x.Username == username && x.VerifyPassword(oldPassword));
                if (existingUser == null)
                {
                    return Result.Failure("Invalid username or password");

                }
                existingUser.HashPassword(newPassword);
                lock (_saveLock)
                {
                    context.SaveChanges();
                }
                return Result.Success();

            }
            catch (Exception ex)
            {
                return Result.Failure<User>(ex.Message);
            }
        }

        public bool UserExists(string username)
        {
            return GetUser(username).IsSuccess;
        }
    }
}
