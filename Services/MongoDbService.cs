using MongoDB.Driver;
using System;
using SocietyManagementSystem.Models;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace SocietyManagementSystem.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(string databaseName)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                IConfiguration configuration = builder.Build();
                var connectionString = configuration["ConnectionStrings:MongoDb"];
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("MongoDB connection string is not configured in appsettings.json");
                }
                
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize MongoDbService: {ex.Message}", ex);
            }
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        public IMongoCollection<Tenant> Tenants => _database.GetCollection<Tenant>("Tenants");

        public IMongoCollection<Rent> Rents => _database.GetCollection<Rent>("Rents");

        public IMongoCollection<MaintenanceRequest> MaintenanceRequests => _database.GetCollection<MaintenanceRequest>("MaintenanceRequests");

        public IMongoCollection<Notice> Notices => _database.GetCollection<Notice>("Notices");

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
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
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await Tenants.Find(_ => true).ToListAsync();
        }

        public async Task<bool> AddTenantAsync(Tenant tenant)
        {
            try
            {
                await Tenants.InsertOneAsync(tenant);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTenantAsync(Tenant tenant)
        {
            try
            {
                var filter = Builders<Tenant>.Filter.Eq(t => t.Id, tenant.Id);
                await Tenants.ReplaceOneAsync(filter, tenant);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTenantAsync(string id)
        {
            try
            {
                var filter = Builders<Tenant>.Filter.Eq(t => t.Id, id);
                await Tenants.DeleteOneAsync(filter);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Rent>> GetAllRentsAsync()
        {
            return await Rents.Find(_ => true).ToListAsync();
        }

        public async Task<List<Rent>> GetRentsByTenantIdAsync(string tenantId)
        {
            return await Rents.Find(r => r.TenantId == tenantId).ToListAsync();
        }

        public async Task<bool> AddRentAsync(Rent rent)
        {
            try
            {
                await Rents.InsertOneAsync(rent);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateRentAsync(Rent rent)
        {
            try
            {
                var filter = Builders<Rent>.Filter.Eq(r => r.Id, rent.Id);
                await Rents.ReplaceOneAsync(filter, rent);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteRentAsync(string id)
        {
            try
            {
                var filter = Builders<Rent>.Filter.Eq(r => r.Id, id);
                await Rents.DeleteOneAsync(filter);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarkRentAsPaidAsync(string id, DateTime paidDate)
        {
            try
            {
                var filter = Builders<Rent>.Filter.Eq(r => r.Id, id);
                var update = Builders<Rent>.Update
                    .Set(r => r.Status, RentStatus.Paid)
                    .Set(r => r.PaidDate, paidDate);
                await Rents.UpdateOneAsync(filter, update);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<MaintenanceRequest>> GetAllMaintenanceRequestsAsync()
        {
            return await MaintenanceRequests.Find(_ => true).ToListAsync();
        }

        public async Task<List<MaintenanceRequest>> GetMaintenanceRequestsByTenantIdAsync(string tenantId)
        {
            return await MaintenanceRequests.Find(m => m.TenantId == tenantId).ToListAsync();
        }

        public async Task<bool> AddMaintenanceRequestAsync(MaintenanceRequest request)
        {
            try
            {
                await MaintenanceRequests.InsertOneAsync(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateMaintenanceRequestAsync(MaintenanceRequest request)
        {
            try
            {
                var filter = Builders<MaintenanceRequest>.Filter.Eq(m => m.Id, request.Id);
                await MaintenanceRequests.ReplaceOneAsync(filter, request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMaintenanceRequestAsync(string id)
        {
            try
            {
                var filter = Builders<MaintenanceRequest>.Filter.Eq(m => m.Id, id);
                await MaintenanceRequests.DeleteOneAsync(filter);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateMaintenanceStatusAsync(string id, MaintenanceStatus status)
        {
            try
            {
                var update = Builders<MaintenanceRequest>.Update
                    .Set(m => m.Status, status);
                if (status == MaintenanceStatus.Closed)
                {
                    update = update.Set(m => m.ResolvedDate, DateTime.Now);
                }
                await MaintenanceRequests.UpdateOneAsync(Builders<MaintenanceRequest>.Filter.Eq(m => m.Id, id), update);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Notice>> GetAllNoticesAsync()
        {
            return await Notices.Find(_ => true).ToListAsync();
        }

        public async Task<List<Notice>> GetActiveNoticesAsync(NoticeAudience? audience = null)
        {
            var filter = Builders<Notice>.Filter.Eq(n => n.IsActive, true);
            if (audience.HasValue)
            {
                filter = Builders<Notice>.Filter.And(filter, Builders<Notice>.Filter.Eq(n => n.Audience, audience.Value));
            }
            return await Notices.Find(filter).ToListAsync();
        }

        public async Task<bool> AddNoticeAsync(Notice notice)
        {
            try
            {
                await Notices.InsertOneAsync(notice);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateNoticeAsync(Notice notice)
        {
            try
            {
                var filter = Builders<Notice>.Filter.Eq(n => n.Id, notice.Id);
                await Notices.ReplaceOneAsync(filter, notice);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteNoticeAsync(string id)
        {
            try
            {
                var filter = Builders<Notice>.Filter.Eq(n => n.Id, id);
                await Notices.DeleteOneAsync(filter);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleNoticeActiveAsync(string id)
        {
            try
            {
                var notice = await Notices.Find(Builders<Notice>.Filter.Eq(n => n.Id, id)).FirstOrDefaultAsync();
                if (notice != null)
                {
                    var update = Builders<Notice>.Update.Set(n => n.IsActive, !notice.IsActive);
                    await Notices.UpdateOneAsync(Builders<Notice>.Filter.Eq(n => n.Id, id), update);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task InitializeIndexesAsync()
        {
            try
            {
                // User indexes
                var userUsernameIndex = new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Username));
                await Users.Indexes.CreateOneAsync(userUsernameIndex);

                // Tenant indexes
                var tenantEmailIndex = new CreateIndexModel<Tenant>(Builders<Tenant>.IndexKeys.Ascending(t => t.Email));
                await Tenants.Indexes.CreateOneAsync(tenantEmailIndex);

                // Rent indexes
                var rentTenantIndex = new CreateIndexModel<Rent>(Builders<Rent>.IndexKeys.Ascending(r => r.TenantId));
                var rentStatusIndex = new CreateIndexModel<Rent>(Builders<Rent>.IndexKeys.Ascending(r => r.Status));
                await Rents.Indexes.CreateOneAsync(rentTenantIndex);
                await Rents.Indexes.CreateOneAsync(rentStatusIndex);

                // Maintenance indexes
                var maintenanceTenantIndex = new CreateIndexModel<MaintenanceRequest>(Builders<MaintenanceRequest>.IndexKeys.Ascending(m => m.TenantId));
                var maintenanceStatusIndex = new CreateIndexModel<MaintenanceRequest>(Builders<MaintenanceRequest>.IndexKeys.Ascending(m => m.Status));
                await MaintenanceRequests.Indexes.CreateOneAsync(maintenanceTenantIndex);
                await MaintenanceRequests.Indexes.CreateOneAsync(maintenanceStatusIndex);

                // Notice indexes
                var noticeActiveIndex = new CreateIndexModel<Notice>(Builders<Notice>.IndexKeys.Ascending(n => n.IsActive));
                var noticeAudienceIndex = new CreateIndexModel<Notice>(Builders<Notice>.IndexKeys.Ascending(n => n.Audience));
                await Notices.Indexes.CreateOneAsync(noticeActiveIndex);
                await Notices.Indexes.CreateOneAsync(noticeAudienceIndex);
            }
            catch (Exception ex)
            {
                // Log or handle index creation errors silently
                Console.WriteLine($"Index initialization error: {ex.Message}");
            }
        }

    }
}
