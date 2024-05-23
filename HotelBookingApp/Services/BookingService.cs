using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using HotelBookingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Couchbase.Query;

namespace HotelBookingApp.Services
{
    public class BookingService : IBookingService
    {
        private readonly INamedBucketProvider _bucketProvider;

        public BookingService(INamedBucketProvider bucketProvider)
        {
            _bucketProvider = bucketProvider;
        }

        public async Task AddBookingAsync(Booking booking)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var collection = bucket.DefaultCollection();
            await collection.InsertAsync(booking.Id, booking);
        }

        public async Task<Booking> GetBookingByIdAsync(string id)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var collection = bucket.DefaultCollection();
            var result = await collection.GetAsync(id);
            return result.ContentAs<Booking>();
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var cluster = bucket.Cluster;
            var queryResult = await cluster.QueryAsync<Booking>("SELECT * FROM `HotelBookingBucket`");

            var bookings = new List<Booking>();
            await foreach (var row in queryResult)
            {
                bookings.Add(row);
            }

            return bookings;
        }

        public async Task UpdateBookingAsync(Booking booking)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var collection = bucket.DefaultCollection();
            await collection.ReplaceAsync(booking.Id, booking);
        }

        public async Task DeleteBookingAsync(string id)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var collection = bucket.DefaultCollection();
            await collection.RemoveAsync(id);
        }
    }
}
