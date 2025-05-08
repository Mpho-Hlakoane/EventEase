using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int VenueId { get; set; } = 0;

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        [StringLength(50)]
        public string BookingReference { get; set; } = string.Empty;

        public Event? Event { get; set; }
        public Venue? Venue { get; set; }
    }
}
