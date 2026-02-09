using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Common.Configuration
{
   
    public static class MongoDbConfiguration
    {
        private static bool _isConfigured = false;
        private static readonly object _lock = new object();

        public static void Configure()
        {
            lock (_lock)
            {
                if (_isConfigured)
                {
                    return; 
                }

                try
                {
                    RegisterEnumSerializer<Gender>();
                    RegisterEnumSerializer<EmploymentType>();
                    RegisterEnumSerializer<EmployeeStatus>();

                    _isConfigured = true;
                    Console.WriteLine("✓ MongoDB enum serialization configured successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Warning: MongoDB enum serialization configuration failed: {ex.Message}");
                }
            }
        }

        private static void RegisterEnumSerializer<TEnum>() where TEnum : struct, Enum
        {
            try
            {
 
                var currentSerializer = BsonSerializer.LookupSerializer<TEnum>();

                if (currentSerializer is not EnumSerializer<TEnum>)
                {
                    BsonSerializer.RegisterSerializer(new EnumSerializer<TEnum>(BsonType.String));
                    Console.WriteLine($"  Registered {typeof(TEnum).Name} as string");
                }
                else
                {
                    Console.WriteLine($"  {typeof(TEnum).Name} already configured");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Could not register {typeof(TEnum).Name}: {ex.Message}");
            }
        }
    }
}