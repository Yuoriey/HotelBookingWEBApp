using HotelBookingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApp.Data
{
    public class HotelBookingAppContext : IdentityDbContext<ApplicationUser>
    {
        public HotelBookingAppContext(DbContextOptions<HotelBookingAppContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Additional configurations
        }
    }
}
