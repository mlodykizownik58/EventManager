using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventManagement.Models;

namespace EventManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventSignup> EventSignups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Określenie złożonego klucza głównego dla EventSignup
            modelBuilder.Entity<EventSignup>()
                .HasKey(es => new { es.EventId, es.UserId });

            // Konfiguracja dla relacji z OrganizerId
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany()  // Może to być null, jeśli nie ma potrzeby wprowadzania tej relacji odwrotnej
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);  // Zapobiegamy usuwaniu organizatora, jeśli istnieją wydarzenia
        }
    }
}
