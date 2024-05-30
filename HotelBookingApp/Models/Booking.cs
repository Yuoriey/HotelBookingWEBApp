using HotelBookingApp.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;

namespace HotelBookingApp.Models
{


public class Booking
{
    [Key]
    public string Id { get; set; }
    public string HotelId { get; set; }
    public virtual Hotel? Hotel { get; set; }
    public string UserId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<Room> Rooms { get; set; }


        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
    }

}