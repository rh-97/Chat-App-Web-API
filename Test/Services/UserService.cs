﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Security.Cryptography;
using System.Text;
using Test.Database;
using Test.Models;

namespace Test.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> collection;


        public UserService(IOptions<DatabaseSettings> _dbsettings)
        {
            var settings = MongoClientSettings.FromConnectionString(_dbsettings.Value.ConnectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var db = client.GetDatabase(_dbsettings.Value.DatabaseName);
            collection = db.GetCollection<User>(_dbsettings.Value.CollectionName);
        }



        // Register New User
        public async Task<bool> Register(User user)
        {
            user.Password = GenerateHash(user.Password);
            return await collection.InsertOneAsync(user).ContinueWith(r => r.IsCompletedSuccessfully);
        }



        // Get a list of users for a given page number and number of users per page
        public async Task<List<User>> Fetch(int itemsPerPage = 15, int pageNumber = 1)
        {
            return await collection.Find(_ => true).Skip((pageNumber-1)*itemsPerPage).Limit(itemsPerPage).ToListAsync();
        }



        // Get all the info for a specific user identified by Id
        public async Task<User> FetchById(string Id)
        {
            var filter = Builders<User>.Filter.Eq<string>("Id", Id);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }


        // Verify login credentials(username & password
        public async Task<bool> VerifyLogin(UserLogin user)
        {
            var filter = (Builders<User>.Filter.Eq<string>("Name", user.Name) & 
                           Builders<User>.Filter.Eq<string>("Password", GenerateHash(user.Password)));
            var res = await collection.Find(filter).FirstOrDefaultAsync();

            if (res == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }



        // Delete a specific user identified by an Id
        public async Task<DeleteResult> Remove(string Id)
        {
            var filter = Builders<User>.Filter.Eq<string>("Id", Id);
            var res = await collection.DeleteOneAsync(filter);
            return res;
        }



        // Update a specific user data 
        public async Task<User> Update(User user)
        {
            return user;
        }


        // Check username is unique or not
        public async Task<bool> IsUniqueUsername(string name)
        {
            var filter = Builders<User>.Filter.Eq<string>("Name", name);
            var user = await collection.FindAsync(filter);
            if (user != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        // Check username is unique or not
        public async Task<bool> IsUniqueEmail(string email)
        {
            var filter = Builders<User>.Filter.Eq<string>("Email", email);
            var user = await collection.FindAsync(filter);
            if (user != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // A helper function to generate password hash
        private string GenerateHash(string password)
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("Learnathon"));
            var temp = Encoding.UTF8.GetBytes(password);
            var hash = hmac.ComputeHash(temp);
            return BitConverter.ToString(hash, 0, hash.Length).Replace("-", String.Empty);
        }

       
    }
}
