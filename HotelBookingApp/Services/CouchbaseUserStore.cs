using Microsoft.AspNetCore.Identity;
using Couchbase;
using Couchbase.KeyValue;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBookingApp.Models;
using Couchbase.Extensions.DependencyInjection;

public class CouchbaseUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserEmailStore<ApplicationUser>, IUserRoleStore<ApplicationUser>
{
    private readonly IBucketProvider _bucketProvider;
    private readonly IBucket _bucket;
    private readonly ICouchbaseCollection _collection;
    private readonly ICouchbaseCollection _userRolesCollection;

    public CouchbaseUserStore(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
        _bucket = _bucketProvider.GetBucketAsync("HotelBookingBucket").Result;
        _collection = _bucket.Scope("identityScope").Collection("users");
        _userRolesCollection = _bucket.Scope("identityScope").Collection("userRoles");

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

    // IUserEmailStore<ApplicationUser> implementation
    public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var query = $"SELECT META().id, * FROM `HotelBookingBucket`.`identityScope`.`users` WHERE LOWER(email) = '{normalizedEmail.ToLower()}' LIMIT 1";
        var result = await _bucket.Cluster.QueryAsync<dynamic>(query);

        var rows = await result.Rows.ToListAsync();
        if (rows.Count > 0)
        {
            var row = rows.First();
            return row["users"].ToObject<ApplicationUser>();
        }

        return null;
    }

    public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email.ToLower());
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
    {
        user.Email = normalizedEmail.ToLower();
        return Task.CompletedTask;
    }



    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        await _userRolesCollection.RemoveAsync(user.Id + "_" + roleName);
    }


    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleName = roleName
        };
        await _userRolesCollection.UpsertAsync($"{user.Id}_{roleName}", userRole);
    }


    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var query = $"SELECT roleName FROM `HotelBookingBucket`.`identityScope`.`userRoles` WHERE userId = '{user.Id}'";
        var result = await _bucket.Cluster.QueryAsync<dynamic>(query);
        var roles = await result.Rows.ToListAsync();
        return roles.Select(row => (string)row.roleName).ToList();
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        var query = $"SELECT roleName FROM `HotelBookingBucket`.`identityScope`.`userRoles` WHERE userId = '{user.Id}' AND roleName = '{roleName}'";
        var result = await _bucket.Cluster.QueryAsync<dynamic>(query);
        var role = await result.Rows.FirstOrDefaultAsync();

        return role != null;
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var query = $"SELECT userId FROM `HotelBookingBucket`.`identityScope`.`userRoles` WHERE roleName = '{roleName}'";
        var result = await _bucket.Cluster.QueryAsync<dynamic>(query);
        var userIds = await result.Rows.ToListAsync();

        var users = new List<ApplicationUser>();
        foreach (var userId in userIds)
        {
            var userResult = await _collection.GetAsync(userId.userId);
            if (userResult != null)
            {
                users.Add(userResult.ContentAs<ApplicationUser>());
            }
        }

        return users;
    }


}
