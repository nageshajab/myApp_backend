using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using BCrypt.Net;
using myazfunction.Models;
namespace myazfunction.DAL
{
    public class UserRepository
    {
        private readonly IMongoCollection<Users> _users;

        public UserRepository(MongoDbContext context)
        {
            _users = context.GetCollection<Users>("users");
        }

        public async Task<List<Users>> GetAllUsersAsync()
        {
            return await _users.Find(user => true).ToListAsync();
        }

        public async Task<Users> GetUserAsync(string id)
        {
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Users> GetUserByUserNameAsync(string username)
        {
            return await _users.Find(user => user.UserName == username).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(Users user)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateUserAsync(string id, Users user)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            await _users.ReplaceOneAsync(user => user.Id == id, user);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _users.DeleteOneAsync(user => user.Id == id);
        }


        public async Task<Users> LoginAsync(string username, string password)
        {
            var user = await _users.Find(u => u.UserName == username).FirstOrDefaultAsync();

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }

            return null;
        }

        public async Task<Users> ChangePasswordAsync(ChangePassword cp)
        {
            var user = await _users.Find(u => u.Id == cp.Id).FirstOrDefaultAsync();

            if (user != null && BCrypt.Net.BCrypt.Verify(cp.currentPassword, user.Password))
            {
                user.Password = cp.password;
                await UpdateUserAsync(cp.Id, user);
            }
            else { }

                return null;
        }


    }


}
