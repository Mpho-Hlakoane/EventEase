using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public required string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        public string ImageUrl { get; set; } = "https://via.placeholder.com/150";
      
        [Required]
        public int EventTypeId { get; set; }
        public EventType? EventType { get; set; }
       
        public ICollection<Booking>? Bookings { get; set; }

    }
}
