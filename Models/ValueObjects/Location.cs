using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.ValueObjects
{
    public class Location
    {
        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("locationName")]
        public string? LocationName { get; set; }

        public Location()
        {
        }

        public Location(double latitude, double longitude, string? address = null, string? locationName = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            Address = address;
            LocationName = locationName;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(LocationName)
                ? LocationName
                : $"{Latitude}, {Longitude}";
        }

        public double DistanceFrom(Location other)
        {
            const double earthRadiusKm = 6371.0;

            var lat1Rad = DegreesToRadians(Latitude);
            var lat2Rad = DegreesToRadians(other.Latitude);
            var deltaLat = DegreesToRadians(other.Latitude - Latitude);
            var deltaLon = DegreesToRadians(other.Longitude - Longitude);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}