using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotelBookingApp.Models;

public class BookingViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string PhoneNumber { get; set; }
    public string HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<Room> SelectedRooms { get; set; } = new List<Room>();
    public List<Room> Rooms { get; set; } = new List<Room>();
}
