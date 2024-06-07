using System.Collections.Generic;

namespace HotelBookingApp.Models
{
    public class HotelListViewModel
    {
        public IEnumerable<Hotel> Hotels { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; }
    }
}
