using LocationService.Models;
using MongoDB.Bson;
using MongoDB.Driver;
namespace LocationService.Services
{
    public class LocationService
    {
        private readonly IMongoCollection<Location> _locations;

        public LocationService(IMongoCollection<Location> locations)
        {
            _locations = locations;
        }

        //public async Task<Location> GetLocationById(string id)
        //{
        //    return await _locations.Find(l => l.Id == id).FirstOrDefaultAsync();
        //}

        //public async Task<Location> GetLocationById(string id)
        //{
        //    var objectId = ObjectId.Parse(id);
        //    var filter = Builders<Location>.Filter.Eq("_id", objectId);
        //    return await _locations.Find(filter).FirstOrDefaultAsync();
        //}

        //public async Task<Location> GetLocationById(string id)
        //{
        //    var objectId = MongoDB.Bson.ObjectId.Parse(id);
        //    var filter = Builders<Location>.Filter.Eq("_id", objectId);
        //    return await _locations.Find(filter).FirstOrDefaultAsync();
        //}

        public async Task<Location> GetLocationById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return null;
            }

            var filter = Builders<Location>.Filter.Eq("_id", objectId);
            return await _locations.Find(filter).FirstOrDefaultAsync();
        }



        public async Task<Location> AddLocation(Location location)
        {
            await _locations.InsertOneAsync(location);
            return location;
        }

        public async Task<List<Location>> GetAllLocationsByEmail(string email)
        {
            return await _locations.Find(l => l.UserEmail == email).ToListAsync();
        }

        public async Task<Location?> UpdateLocation(string id, Location updated)
        {
            updated.Id = id;
            var result = await _locations.ReplaceOneAsync(l => l.Id == id, updated);
            return result.IsAcknowledged ? updated : null;
        }

        public async Task<bool> DeleteLocation(string id)
        {
            var result = await _locations.DeleteOneAsync(l => l.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
