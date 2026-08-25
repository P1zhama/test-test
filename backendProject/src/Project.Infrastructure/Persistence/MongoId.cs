using MongoDB.Bson;

namespace Project.Infrastructure.Persistence
{
    public static class MongoId
    {
        public static bool IsValid(string? id) => !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);

        public static ObjectId Parse(string id) => ObjectId.Parse(id);

        public static string NewId() => ObjectId.GenerateNewId().ToString();
    }
}
