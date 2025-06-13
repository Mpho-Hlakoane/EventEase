using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetailsView> BookingDetailsViews { get; set; }

        // Add EventTypes DbSet
        public DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique index on venue bookings within date range
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.VenueId, b.StartDate, b.EndDate })
                .IsUnique();

            // Relationships for Booking -> Venue and Event
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // BookingDetailsView mapping (no key, maps to DB view)
            modelBuilder.Entity<BookingDetailsView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("BookingDetailsView");
                entity.Property(v => v.BookingId).HasColumnName("BookingId");
                // Map other properties if needed
            });

            // Seed EventTypes data separately (not inside BookingDetailsView)
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, Name = "Conference" },
                new EventType { EventTypeId = 2, Name = "Wedding" },
                new EventType { EventTypeId = 3, Name = "Concert" },
                new EventType { EventTypeId = 4, Name = "Meetup" }
            );
        }
    }
}
