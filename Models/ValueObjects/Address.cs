using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.ValueObjects
{
    public class Address
    {
        [BsonElement("street")]
        public string Street { get; set; } = string.Empty;

        [BsonElement("city")]
        public string City { get; set; } = string.Empty;

        [BsonElement("state")]
        public string State { get; set; } = string.Empty;

        [BsonElement("country")]
        public string Country { get; set; } = string.Empty;

        [BsonElement("postalCode")]
        public string PostalCode { get; set; } = string.Empty;

        public Address()
        {
        }

        public Address(string street, string city, string state, string country, string postalCode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            PostalCode = postalCode;
        }

        public override string ToString()
        {
            return $"{Street}, {City}, {State}, {Country} - {PostalCode}";
        }
    }
}