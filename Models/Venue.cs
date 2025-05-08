using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

    [Required]
        [StringLength(100)]
        public string Name { get; set; } = "Unknown Venue";

        [Required]
        [StringLength(300)]
        public string Location { get; set; } = "Unknown Location";

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }
        public string? ImageUrl { get; set; } = "https://via.placeholder.com/150";
        public ICollection<Booking>? Bookings { get; set; } = new List<Booking>();
    }
}