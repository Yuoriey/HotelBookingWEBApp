using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using HotelBookingApp.Models;
using HotelBookingApp.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HotelService : IHotelService
{
    private readonly IBucketProvider _bucketProvider;
    private readonly IBucket _bucket;
    private readonly ICouchbaseCollection _collection;

    public HotelService(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
        _bucket = _bucketProvider.GetBucketAsync("HotelBookingBucket").Result;
        _collection = _bucket.DefaultCollection();
    }

    public async Task AddHotelAsync(Hotel hotel)
    {
        await _collection.UpsertAsync(hotel.Id, hotel);
    }

    public async Task DeleteHotelAsync(string id)
    {
        await _collection.RemoveAsync(id);
    }

    public async Task<IEnumerable<Hotel>> GetAllHotelsAsync()
    {
        var query = $"SELECT META().id, name, location, rating, picture, isActive, rooms FROM `HotelBookingBucket`.`_default`.`_default`";
        var result = await _bucket.Cluster.QueryAsync<dynamic>(query);
        var hotels = await result.Rows.ToListAsync();
        return hotels.Select(row => new Hotel
        {
            Id = row.id,
            Name = row.name,
            Location = row.location,
            Rating = row.rating,
            Picture = row.picture,
            IsActive = row.isActive,
            Rooms = ((IEnumerable<dynamic>)row.rooms).Select(r => new Room
            {
                Type = r.type,
                AvailableRooms = r.availableRooms,
                Price = r.price,
                Picture = r.picture,
                IsAvailable = r.isAvailable
            }).ToList()
        });
    }

    public async Task<Hotel> GetHotelByIdAsync(string id)
    {
        var result = await _collection.GetAsync(id);
        return result.ContentAs<Hotel>();
    }

    public async Task<IEnumerable<Hotel>> SearchHotelsAsync(string searchQuery)
    {
        var query = $"SELECT META().id, name, location, rating, picture, isActive, rooms FROM `HotelBookingBucket`.`_default`.`_default` WHERE LOWER(name) LIKE '%{searchQuery.ToLower()}%'";
        var result = await _bucket.Cluster.QueryAsync<dynamic>(query);
        var hotels = await result.Rows.ToListAsync();
        return hotels.Select(row => new Hotel
        {
            Id = row.id,
            Name = row.name,
            Location = row.location,
            Rating = row.rating,
            Picture = row.picture,
            IsActive = row.isActive,
            Rooms = ((IEnumerable<dynamic>)row.rooms).Select(r => new Room
            {
                Type = r.type,
                AvailableRooms = r.availableRooms,
                Price = r.price,
                Picture = r.picture,
                IsAvailable = r.isAvailable
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
        await _collection.UpsertAsync(hotel.Id, hotel);
    }
}
