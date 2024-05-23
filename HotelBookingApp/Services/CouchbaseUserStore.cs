using Microsoft.AspNetCore.Identity;
using Couchbase;
using Couchbase.KeyValue;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBookingApp.Models;
using Couchbase.Extensions.DependencyInjection;

public class CouchbaseUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>
{
    private readonly IBucketProvider _bucketProvider;
    private readonly IBucket _bucket;
    private readonly ICouchbaseCollection _collection;

    public CouchbaseUserStore(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
        _bucket = _bucketProvider.GetBucketAsync("HotelBookingBucket").Result;
        _collection = _bucket.Scope("identityScope").Collection("users");
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await _collection.UpsertAsync(user.Id, user);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await _collection.RemoveAsync(user.Id);
        return IdentityResult.Success;
    }

    public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var result = await _collection.GetAsync(userId);
        return result.ContentAs<ApplicationUser>();
    }

    public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var query = $"SELECT META().id, * FROM `HotelBookingBucket`.`identityScope`.`users` WHERE LOWER(email) = '{normalizedUserName.ToLower()}' LIMIT 1";
        var result = await _bucket.Cluster.QueryAsync<dynamic>(query);

        var rows = await result.Rows.ToListAsync();
        if (rows.Count > 0)
        {
            var row = rows.First();
            return row["users"].ToObject<ApplicationUser>();
        }

        return null;
    }

    public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email.ToLower());
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.Email = normalizedName.ToLower();
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
    {
        user.Email = userName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await _collection.UpsertAsync(user.Id, user);
        return IdentityResult.Success;
    }

    public void Dispose()
    {
        // Dispose any resources if needed.
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }
}
