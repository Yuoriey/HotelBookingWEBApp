using Couchbase;
using Couchbase.Core.Exceptions;
using Couchbase.Management.Buckets;
using Couchbase.Management.Collections;
using System.Threading.Tasks;

public class CouchbaseSetup
{
    private readonly string _connectionString;
    private readonly string _username;
    private readonly string _password;
    private readonly string _bucketName;

    public CouchbaseSetup(string connectionString, string username, string password, string bucketName)
    {
        _connectionString = connectionString;
        _username = username;
        _password = password;
        _bucketName = bucketName;
    }

    public async Task SetupCouchbaseAsync()
    {
        var cluster = await Cluster.ConnectAsync(_connectionString, _username, _password);

        var bucketManager = cluster.Buckets;

        // Check if the bucket exists
        var bucketExists = false;
        try
        {
            await bucketManager.GetBucketAsync(_bucketName);
            bucketExists = true;
        }
        catch (Couchbase.Management.Buckets.BucketNotFoundException)
        {
            // The bucket does not exist
        }

        if (!bucketExists)
        {
            var bucketSettings = new BucketSettings
            {
                Name = _bucketName,
                BucketType = BucketType.Couchbase,
                RamQuotaMB = 100
            };
            await bucketManager.CreateBucketAsync(bucketSettings);
        }

        var bucket = await cluster.BucketAsync(_bucketName);
        var collectionManager = bucket.Collections;

        // Check if the scope exists
        var scopeExists = false;
        try
        {
            await collectionManager.GetScopeAsync("identityScope");
            scopeExists = true;
        }
        catch (ScopeNotFoundException)
        {
            // The scope does not exist
        }

        if (!scopeExists)
        {
            await collectionManager.CreateScopeAsync("identityScope");
        }

        // Check if the collections exist
        var collections = await collectionManager.GetAllScopesAsync();
        var identityScope = collections.FirstOrDefault(s => s.Name == "identityScope");

        if (identityScope != null)
        {
            var userCollectionExists = identityScope.Collections.Any(c => c.Name == "users");
            var roleCollectionExists = identityScope.Collections.Any(c => c.Name == "roles");

            if (!userCollectionExists)
            {
                await collectionManager.CreateCollectionAsync(new CollectionSpec("identityScope", "users"));
            }

            if (!roleCollectionExists)
            {
                await collectionManager.CreateCollectionAsync(new CollectionSpec("identityScope", "roles"));
            }
        }

        await cluster.DisposeAsync();
    }
}
