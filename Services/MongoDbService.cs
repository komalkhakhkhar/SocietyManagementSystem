using MongoDB.Driver;
using System;
using WPF.Models;

namespace WPF.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService()
        {
            // Connection string - replace with your actual MongoDB connection string
            var connectionString = "mongodb+srv://kanjariyamanthan2002_db_user:4PH4fTC5Mkz8dvg5@cluster2002.di29jo7.mongodb.net/?retryWrites=true&w=majority&appName=Cluster2002"; // For local MongoDB
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("WpfAppDb");
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                await Users.InsertOneAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidateLoginAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            return user != null && user.Password == password; // In real app, compare hashed passwords
        }
    }
}