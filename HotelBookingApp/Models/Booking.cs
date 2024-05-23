using System.ComponentModel.DataAnnotations;

public class Booking
{
    [Key]
    public string Id { get; set; }
    public string HotelId { get; set; }
    public string UserId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
}
