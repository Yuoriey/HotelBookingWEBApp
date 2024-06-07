using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using HotelBookingApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Query;
using Couchbase.Management.Collections;

namespace HotelBookingApp.Services
{
    public class BookingService : IBookingService
    {
        private readonly INamedBucketProvider _bucketProvider;
        private readonly string _bucketName = "HotelBookingBucket";
        private readonly string _scopeName = "Reservations";
        private readonly string _collectionName = "Bookings";

        public BookingService(INamedBucketProvider bucketProvider)
        {
            _bucketProvider = bucketProvider;
        }

        public async Task AddBookingAsync(Booking booking)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            await EnsureScopeAndCollectionExistAsync(bucket);
            var scope = bucket.Scope(_scopeName);
            var collection = scope.Collection(_collectionName);

            await collection.UpsertAsync(booking.Id, booking);
        }

        public async Task<Booking> GetBookingByIdAsync(string id)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var scope = bucket.Scope(_scopeName);
            var collection = scope.Collection(_collectionName);
            var result = await collection.GetAsync(id);
            return result.ContentAs<Booking>();
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var query = $"SELECT b.* FROM `{_bucketName}`.`{_scopeName}`.`{_collectionName}` b";
            var result = await bucket.Cluster.QueryAsync<Booking>(query);

            var bookings = new List<Booking>();
            await foreach (var row in result.Rows)
            {
                bookings.Add(row);
            }

            return bookings;
        }

        public async Task UpdateBookingAsync(Booking booking)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var scope = bucket.Scope(_scopeName);
            var collection = scope.Collection(_collectionName);
            await collection.ReplaceAsync(booking.Id, booking);
        }

        public async Task DeleteBookingAsync(string id)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var scope = bucket.Scope(_scopeName);
            var collection = scope.Collection(_collectionName);
            await collection.RemoveAsync(id);
        }

        private async Task EnsureScopeAndCollectionExistAsync(IBucket bucket)
        {
            // Check if scope exists
            var scopes = await bucket.Collections.GetAllScopesAsync();
            var scopeExists = scopes.Any(s => s.Name == _scopeName);

            if (!scopeExists)
            {
                // Create the scope
                await bucket.Collections.CreateScopeAsync(_scopeName);
            }

            // Check if collection exists within the scope
            var collections = scopes.FirstOrDefault(s => s.Name == _scopeName)?.Collections;
            var collectionExists = collections?.Any(c => c.Name == _collectionName) ?? false;

            if (!collectionExists)
            {
                // Create the collection
                await bucket.Collections.CreateCollectionAsync(new CollectionSpec(_scopeName, _collectionName));
            }
        }
        public async Task<IEnumerable<Booking>> GetBookingsByUserAsync(string userId)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var query = $"SELECT b.* FROM `{_bucketName}`.`{_scopeName}`.`{_collectionName}` b WHERE b.userId = '{userId}'";
            var result = await bucket.Cluster.QueryAsync<Booking>(query);

            var bookings = new List<Booking>();
            await foreach (var row in result.Rows)
            {
                bookings.Add(row);
            }

            return bookings;
        }

    }
}
