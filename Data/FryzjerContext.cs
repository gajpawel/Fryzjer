using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Models;

namespace Fryzjer.Data
{
    public class FryzjerContext : DbContext
    {
        public FryzjerContext (DbContextOptions<FryzjerContext> options)
            : base(options)
        {
        }

        public DbSet<Fryzjer.Models.Client> Client { get; set; } = default!;
        
        public DbSet<Fryzjer.Models.Reservation> Reservation { get; set; } = default!;
        public DbSet<Fryzjer.Models.Hairdresser> Hairdresser { get; set; } = default!;
        public DbSet<Fryzjer.Models.Place> Place { get; set; } = default!;
        public DbSet<Fryzjer.Models.Service> Service { get; set; } = default!;
        public DbSet<Fryzjer.Models.Specialization> Specialization { get; set; } = default!;
        public DbSet<Fryzjer.Models.Administrator> Administrator { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
         .SelectMany(t => t.GetProperties())
         .Where(p => p.ClrType == typeof(string)))
            {
                property.SetColumnType("TEXT"); // Ustaw TEXT dla wszystkich stringów
            }

            base.OnModelCreating(modelBuilder);

            // Relacje w modelu

            // Reservation - Client
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Client)
                .WithMany()
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reservation - Hairdresser
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Hairdresser)
                .WithMany()
                .HasForeignKey(r => r.HairdresserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation - Service
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Service)
                .WithMany()
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hairdresser - Place
            modelBuilder.Entity<Hairdresser>()
                .HasOne(r => r.Place)
                .WithMany()
                .HasForeignKey(r => r.PlaceId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Specialization - Hairdresser
            modelBuilder.Entity<Specialization>()
                .HasOne(r => r.Hairdresser)
                .WithMany()
                .HasForeignKey(r => r.HairdresserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Specialization - Service
            modelBuilder.Entity<Specialization>()
                .HasOne(r => r.Service)
                .WithMany()
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
