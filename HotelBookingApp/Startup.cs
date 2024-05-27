using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HotelBookingApp.Services;
using HotelBookingApp.Models;
using Microsoft.AspNetCore.Identity;
using DinkToPdf;
using DinkToPdf.Contracts;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {

        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

        services.AddControllersWithViews();

        var couchbaseConfig = Configuration.GetSection("Couchbase");
        var connectionString = couchbaseConfig["ConnectionString"];
        var username = couchbaseConfig["Username"];
        var password = couchbaseConfig["Password"];
        var bucketName = couchbaseConfig["BucketName"];

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException("Couchbase configuration is incomplete.");
        }

        services.AddCouchbase(options =>
        {
            options.ConnectionString = connectionString;
            options.UserName = username;
            options.Password = password;
            options.KvTimeout = TimeSpan.FromSeconds(5);
            options.ManagementTimeout = TimeSpan.FromSeconds(10);
        });

        services.AddCouchbaseBucket<INamedBucketProvider>(bucketName);

        // Add application services
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingServiceFactory, BookingServiceFactory>();
        services.AddScoped<IPaymentService, MockPaymentService>();


        // Identity configuration
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddUserStore<CouchbaseUserStore>()
            .AddRoleStore<CouchbaseRoleStore>()
            .AddDefaultTokenProviders();

        services.AddRazorPages();

        services.AddCors(c =>
        {
            c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        services.AddHttpClient();
        services.AddSignalR();
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ICouchbaseLifetimeService couchbaseLifetimeService)
    {
        app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapRazorPages();
        });

        // Setup Couchbase
        var couchbaseSetup = new CouchbaseSetup(
            Configuration["Couchbase:ConnectionString"],
            Configuration["Couchbase:Username"],
            Configuration["Couchbase:Password"],
            Configuration["Couchbase:BucketName"]);

        couchbaseSetup.SetupCouchbaseAsync().Wait();
    }
}
