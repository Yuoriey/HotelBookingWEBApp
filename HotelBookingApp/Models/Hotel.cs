using System.ComponentModel.DataAnnotations;

namespace HotelBookingApp.Models
{
    public class Hotel
    {
        [Key]
        public string Id { get; set; }
        [Required(ErrorMessage = "The Name field is required.")]
        [StringLength(100)]
        public string Name { get; set; }
        [Required(ErrorMessage = "The Locaion field is required")]
        [StringLength(100)]
        public string Location { get; set; }
        [Required(ErrorMessage = "The Rating must be between 0 and 5")]
        [Range(0, 5)]
        public double Rating { get; set; }
        public string Picture { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public List<Room>Rooms { get; set; } = new List<Room>();
    }

    public class Room
    {
        [Required(ErrorMessage = "The type fiels is required")]
        public string Type { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "The AvailableRooms field must be a non-negative integer.")]
        public int AvailableRooms { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "The Price field must be a non-negative number.")]
        public double Price { get; set; }
        public string Picture { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}
