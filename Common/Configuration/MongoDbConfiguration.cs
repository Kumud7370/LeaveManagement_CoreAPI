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
                    Console.WriteLine("⚠️ MongoDbConfiguration.Configure() called again - already configured, skipping");
                    return;
                }

                Console.WriteLine("═══════════════════════════════════════════════════");
                Console.WriteLine("🔧 STARTING MongoDB Enum Configuration");
                Console.WriteLine("═══════════════════════════════════════════════════");

                try
                {
                    // IMPORTANT: Unregister any existing serializers first
                    UnregisterIfExists<Gender>();
                    UnregisterIfExists<EmploymentType>();
                    UnregisterIfExists<EmployeeStatus>();

                    // Now register our string serializers
                    RegisterEnumSerializer<Gender>();
                    RegisterEnumSerializer<EmploymentType>();
                    RegisterEnumSerializer<EmployeeStatus>();

                    _isConfigured = true;

                    Console.WriteLine("═══════════════════════════════════════════════════");
                    Console.WriteLine("✅ MongoDB enum serialization configured successfully");
                    Console.WriteLine("   All enums will be stored as STRINGS in MongoDB");
                    Console.WriteLine("═══════════════════════════════════════════════════\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("═══════════════════════════════════════════════════");
                    Console.WriteLine($"❌ CRITICAL ERROR: MongoDB enum configuration failed!");
                    Console.WriteLine($"   Error: {ex.Message}");
                    Console.WriteLine($"   Stack: {ex.StackTrace}");
                    Console.WriteLine("═══════════════════════════════════════════════════\n");
                    throw;
                }
            }
        }

        private static void UnregisterIfExists<TEnum>() where TEnum : struct, Enum
        {
            try
            {
                // Check if a serializer is already registered
                var existingSerializer = BsonSerializer.SerializerRegistry.GetSerializer<TEnum>();

                if (existingSerializer != null)
                {
                    Console.WriteLine($"  ⚠️  {typeof(TEnum).Name} serializer already exists");

                    // Check what type it is
                    if (existingSerializer is EnumSerializer<TEnum> enumSer)
                    {
                        Console.WriteLine($"      Existing representation: {enumSer.Representation}");
                    }
                }
            }
            catch
            {
                // No serializer registered yet - this is fine
                Console.WriteLine($"  ℹ️  {typeof(TEnum).Name} - no existing serializer");
            }
        }

        private static void RegisterEnumSerializer<TEnum>() where TEnum : struct, Enum
        {
            try
            {
                var serializer = new EnumSerializer<TEnum>(BsonType.String);
                BsonSerializer.RegisterSerializer(serializer);

                // Verify it was registered correctly
                var registered = BsonSerializer.SerializerRegistry.GetSerializer<TEnum>();
                if (registered is EnumSerializer<TEnum> regEnum)
                {
                    Console.WriteLine($"  ✅ {typeof(TEnum).Name} → Registered as {regEnum.Representation}");

                    if (regEnum.Representation != BsonType.String)
                    {
                        Console.WriteLine($"      ⚠️ WARNING: Registered but NOT as String!");
                    }
                }
                else
                {
                    Console.WriteLine($"  ❌ {typeof(TEnum).Name} → Registration FAILED");
                }
            }
            catch (BsonSerializationException ex) when (ex.Message.Contains("already registered"))
            {
                Console.WriteLine($"  ⚠️  {typeof(TEnum).Name} → Already registered (attempting to override)");

                // Try to get the existing one and check its configuration
                try
                {
                    var existing = BsonSerializer.SerializerRegistry.GetSerializer<TEnum>();
                    if (existing is EnumSerializer<TEnum> existingEnum)
                    {
                        Console.WriteLine($"      Existing configuration: {existingEnum.Representation}");

                        if (existingEnum.Representation == BsonType.String)
                        {
                            Console.WriteLine($"      ✅ Already configured correctly as String");
                        }
                        else
                        {
                            Console.WriteLine($"      ❌ PROBLEM: Configured as {existingEnum.Representation}, not String!");
                            Console.WriteLine($"      ❌ This is why new employees are saved as integers!");
                        }
                    }
                }
                catch (Exception checkEx)
                {
                    Console.WriteLine($"      ❌ Could not check existing serializer: {checkEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ {typeof(TEnum).Name} → ERROR: {ex.Message}");
                throw;
            }
        }
    }
}