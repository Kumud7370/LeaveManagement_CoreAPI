using MongoDB.Driver;
using MongoDB.Bson;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Data.Interfaces;

namespace AttendanceManagementSystem.Data.Migrations
{
    public class EnumMigrationService
    {
        private readonly IMongoDbContext _context;
        private readonly ILogger<EnumMigrationService> _logger;

        public EnumMigrationService(IMongoDbContext context, ILogger<EnumMigrationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task MigrateEnumsToStringsAsync()
        {
            try
            {
                _logger.LogInformation("🔄 Starting enum migration to strings...");

                var employeeCollection = _context.GetCollection<BsonDocument>("employees");

                var filter = Builders<BsonDocument>.Filter.Or(
                    Builders<BsonDocument>.Filter.Type("Gender", BsonType.Int32),
                    Builders<BsonDocument>.Filter.Type("EmploymentType", BsonType.Int32),
                    Builders<BsonDocument>.Filter.Type("EmployeeStatus", BsonType.Int32)
                );

                var employeesNeedingMigration = await employeeCollection.CountDocumentsAsync(filter);

                if (employeesNeedingMigration == 0)
                {
                    _logger.LogInformation("✓ No migration needed - all enums are already strings");
                    return;
                }

                _logger.LogInformation($"📊 Found {employeesNeedingMigration} employee(s) with integer enums");

                var allEmployees = await employeeCollection.Find(filter).ToListAsync();
                int updatedCount = 0;

                foreach (var employee in allEmployees)
                {
                    var updateDefinitions = new List<UpdateDefinition<BsonDocument>>();

                    if (employee.Contains("Gender") && employee["Gender"].IsInt32)
                    {
                        var genderValue = (Gender)employee["Gender"].AsInt32;
                        updateDefinitions.Add(Builders<BsonDocument>.Update.Set("Gender", genderValue.ToString()));
                        _logger.LogDebug($"  Converting Gender: {employee["Gender"].AsInt32} → {genderValue}");
                    }

                    if (employee.Contains("EmploymentType") && employee["EmploymentType"].IsInt32)
                    {
                        var employmentTypeValue = (EmploymentType)employee["EmploymentType"].AsInt32;
                        updateDefinitions.Add(Builders<BsonDocument>.Update.Set("EmploymentType", employmentTypeValue.ToString()));
                        _logger.LogDebug($"  Converting EmploymentType: {employee["EmploymentType"].AsInt32} → {employmentTypeValue}");
                    }

                    if (employee.Contains("EmployeeStatus") && employee["EmployeeStatus"].IsInt32)
                    {
                        var employeeStatusValue = (EmployeeStatus)employee["EmployeeStatus"].AsInt32;
                        updateDefinitions.Add(Builders<BsonDocument>.Update.Set("EmployeeStatus", employeeStatusValue.ToString()));
                        _logger.LogDebug($"  Converting EmployeeStatus: {employee["EmployeeStatus"].AsInt32} → {employeeStatusValue}");
                    }

                    if (updateDefinitions.Count > 0)
                    {
                        var employeeFilter = Builders<BsonDocument>.Filter.Eq("_id", employee["_id"]);
                        var update = Builders<BsonDocument>.Update.Combine(updateDefinitions);

                        await employeeCollection.UpdateOneAsync(employeeFilter, update);
                        updatedCount++;

                        var employeeCode = employee.Contains("employeeCode")
                            ? employee["employeeCode"].AsString
                            : "Unknown";
                        _logger.LogInformation($"  ✓ Updated employee: {employeeCode}");
                    }
                }

                _logger.LogInformation($"✅ Enum migration completed! Updated {updatedCount} employee(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Enum migration failed: {Message}", ex.Message);
            }
        }
    }
}