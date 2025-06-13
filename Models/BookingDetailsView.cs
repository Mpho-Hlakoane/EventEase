namespace EventEase.Models
{
    public class BookingDetailsView
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }

        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string VenueLocation { get; set; } = string.Empty;
        public int VenueCapacity { get; set; }
    }

}
