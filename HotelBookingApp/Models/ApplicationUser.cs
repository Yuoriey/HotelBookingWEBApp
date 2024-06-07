using Microsoft.AspNetCore.Identity;

namespace HotelBookingApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<string> Roles { get; set; } = new List<string>();
    }
}
