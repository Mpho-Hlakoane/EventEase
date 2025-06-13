using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Venue name must be less than 100 characters.")]
        public string Name { get; set; } = "Unknown Venue";

        [Required]
        [StringLength(300, ErrorMessage = "Location must be less than 300 characters.")]
        public string Location { get; set; } = "Unknown Location";

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        [Display(Name = "Venue Image URL")]
        public string ImageUrl { get; set; } = "https://via.placeholder.com/150";

        // Navigation property for related bookings
        public ICollection<Booking>? Bookings { get; set; } = new List<Booking>();
    }
}
