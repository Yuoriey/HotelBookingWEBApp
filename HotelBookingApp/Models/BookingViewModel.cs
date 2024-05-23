using System;
using HotelBookingApp.Models;

public class BookingViewModel
{
    public string HotelId { get; set; }
    public Hotel Hotel { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string StripeToken { get; set; }
    public List<Room> Rooms { get; set; } = new List<Room>();
}
