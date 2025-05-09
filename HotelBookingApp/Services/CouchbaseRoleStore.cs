﻿using Microsoft.AspNetCore.Identity;
using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class CouchbaseRoleStore : IRoleStore<IdentityRole>, IQueryableRoleStore<IdentityRole>
{
    private readonly IBucketProvider _bucketProvider;
    private readonly IBucket _bucket;
    private readonly ICouchbaseCollection _collection;

    public CouchbaseRoleStore(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
        _bucket = _bucketProvider.GetBucketAsync("HotelBookingBucket").Result;
        _collection = _bucket.Scope("identityScope").Collection("roles");
    }

    public IQueryable<IdentityRole> Roles => GetAllRolesAsync().Result.AsQueryable();

    public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        await _collection.UpsertAsync(role.Id, role);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        await _collection.RemoveAsync(role.Id);
        return IdentityResult.Success;
    }

    public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        var result = await _collection.GetAsync(roleId);
        return result.ContentAs<IdentityRole>();
    }

    public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var query = $"SELECT * FROM `HotelBookingBucket`.`identityScope`.`roles` WHERE LOWER(name) = '{normalizedRoleName.ToLower()}' LIMIT 1";
        var result = await _bucket.Cluster.QueryAsync<dynamic>(query);
        var role = await result.Rows.FirstOrDefaultAsync();
        return role?.roles.ToObject<IdentityRole>();
    }

    public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name.ToLower());
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id);
    }

    public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.Name = normalizedName.ToLower();
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        await _collection.UpsertAsync(role.Id, role);
        return IdentityResult.Success;
    }

    public void Dispose()
    {
        // Dispose any resources if needed.
    }

    private async Task<IEnumerable<IdentityRole>> GetAllRolesAsync()
    {
        var query = $"SELECT META().id, * FROM `HotelBookingBucket`.`identityScope`.`roles`";
        var result = await _bucket.Cluster.QueryAsync<dynamic>(query);
        var roles = await result.Rows.ToListAsync();

        return roles.Select(row => new IdentityRole
        {
            Id = row.id,
            Name = row.name
        });
    }
}
