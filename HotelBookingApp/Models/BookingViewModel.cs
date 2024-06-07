using HotelBookingApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class BookingViewModel
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Address { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    [StringLength(10)]
    public string ZipCode { get; set; }
    [Required]
    public string Country { get; set; }
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
    public string? UserId { get; set; }
    [Required]
    public string HotelId { get; set; }
    public virtual Hotel? Hotel { get; set; }
    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }
    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    [Required]
    public IFormFile? PaymentProof { get; set; }
    public List<Room> SelectedRooms { get; set; } = new List<Room>();
    public List<Room> Rooms { get; set; } = new List<Room>();
}
