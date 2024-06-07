using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using HotelBookingApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HotelBookingApp.Services;

public class HotelService : IHotelService
{
    private readonly IBucketProvider _bucketProvider;
    private readonly string _bucketName;
    private readonly ILogger<HotelService> _logger;

    public HotelService(IBucketProvider bucketProvider, IConfiguration configuration, ILogger<HotelService> logger)
    {
        _bucketProvider = bucketProvider;
        _bucketName = configuration["Couchbase:BucketName"];
        _logger = logger;
    }

    public async Task AddHotelAsync(Hotel hotel)
    {
        var bucket = await _bucketProvider.GetBucketAsync(_bucketName);
        var collection = bucket.DefaultCollection();
        await collection.UpsertAsync(hotel.Id, hotel);
    }

    public async Task DeleteHotelAsync(string id)
    {
        var bucket = await _bucketProvider.GetBucketAsync(_bucketName);
        var collection = bucket.DefaultCollection();
        await collection.RemoveAsync(id);
    }

    public async Task<IEnumerable<Hotel>> GetAllHotelsAsync()
    {
        var bucket = await _bucketProvider.GetBucketAsync(_bucketName);
        var query = $"SELECT META().id, name, location, rating, picture, isActive, rooms FROM `{_bucketName}`";
        var result = await bucket.Cluster.QueryAsync<dynamic>(query);

        if (result == null)
        {
            _logger.LogError("Query result is null.");
            return new List<Hotel>();
        }

        var rows = await result.Rows.ToListAsync();

        if (rows == null || !rows.Any())
        {
            _logger.LogWarning("No rows returned from query.");
            return new List<Hotel>();
        }

        return rows.Select(row => new Hotel
        {
            Id = row.id,
            Name = row.name,
            Location = row.location,
            Rating = row.rating ?? 0, // Ensuring rating is not null
            Picture = row.picture ?? string.Empty,
            IsActive = row.isActive ?? false,
            Rooms = ((IEnumerable<dynamic>)row.rooms).Select(r => new Room
            {
                Type = r.type,
                AvailableRooms = r.availableRooms ?? 0,
                Price = r.price ?? 0.0,
                Picture = r.picture ?? string.Empty,
                IsAvailable = r.isAvailable ?? false
            }).ToList()
        });
    }

    public async Task<Hotel> GetHotelByIdAsync(string id)
    {
        var bucket = await _bucketProvider.GetBucketAsync(_bucketName);
        var collection = bucket.DefaultCollection();
        var result = await collection.GetAsync(id);
        return result.ContentAs<Hotel>();
    }

    public async Task<IEnumerable<Hotel>> SearchHotelsAsync(string searchQuery)
    {
        var bucket = await _bucketProvider.GetBucketAsync(_bucketName);
        var query = $"SELECT META().id, name, location, rating, picture, isActive, rooms FROM `{_bucketName}` WHERE LOWER(name) LIKE '%{searchQuery.ToLower()}%'";
        var result = await bucket.Cluster.QueryAsync<dynamic>(query);

        if (result == null)
        {
            _logger.LogError("Query result is null.");
            return new List<Hotel>();
        }

        var rows = await result.Rows.ToListAsync();

        if (rows == null || !rows.Any())
        {
            _logger.LogWarning("No rows returned from query.");
            return new List<Hotel>();
        }

        return rows.Select(row => new Hotel
        {
            Id = row.id,
            Name = row.name,
            Location = row.location,
            Rating = row.rating ?? 0, // Ensuring rating is not null
            Picture = row.picture ?? string.Empty,
            IsActive = row.isActive ?? false,
            Rooms = ((IEnumerable<dynamic>)row.rooms).Select(r => new Room
            {
                Type = r.type,
                AvailableRooms = r.availableRooms ?? 0,
                Price = r.price ?? 0.0,
                Picture = r.picture ?? string.Empty,
                IsAvailable = r.isAvailable ?? false
            }).ToList()
        });
    }

    public async Task SetHotelStatusAsync(string id, bool isActive)
    {
        var hotel = await GetHotelByIdAsync(id);
        if (hotel != null)
        {
            hotel.IsActive = isActive;
            await UpdateHotelAsync(hotel);
        }
    }

    public async Task UpdateHotelAsync(Hotel hotel)
    {
        var bucket = await _bucketProvider.GetBucketAsync(_bucketName);
        var collection = bucket.DefaultCollection();
        await collection.UpsertAsync(hotel.Id, hotel);
    }

    public async Task<Room> GetRoomByTypeAsync(string hotelId, string roomType)
    {
        var bucket = await _bucketProvider.GetBucketAsync(_bucketName);
        var collection = bucket.DefaultCollection();
        var query = $"SELECT rooms FROM `{_bucketName}` USE KEYS '{hotelId}'";
        var result = await bucket.Cluster.QueryAsync<dynamic>(query);

        if (result == null)
        {
            _logger.LogError("Query result is null.");
            return null;
        }

        Room room = null;

        await foreach (var row in result.Rows)
        {
            var rooms = row.rooms as IEnumerable<dynamic>;
            var foundRoom = rooms?.FirstOrDefault(r => r.type == roomType);

            if (foundRoom != null)
            {
                room = new Room
                {
                    Type = foundRoom.type,
                    AvailableRooms = foundRoom.availableRooms ?? 0,
                    Price = foundRoom.price ?? 0.0,
                    Picture = foundRoom.picture ?? string.Empty,
                    IsAvailable = foundRoom.isAvailable ?? false,
                    NumberOfGuests = foundRoom.numberOfGuests ?? 1
                };
                break;
            }
        }

        return room;
    }


}
