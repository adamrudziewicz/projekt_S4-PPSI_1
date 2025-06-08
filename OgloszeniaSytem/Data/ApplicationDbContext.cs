using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OgloszeniaSytem.Models;

namespace OgloszeniaSytem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Listing> Ogloszenia { get; set; }
        public DbSet<Category> Kategorie { get; set; }
        public DbSet<Location> Lokalizacje { get; set; }
        public DbSet<Answer> Odpowiedzi { get; set; }
        public DbSet<Photo> Zdjecia { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Konfiguracja relacji
            builder.Entity<Listing>()
                .HasOne(o => o.Autor)
                .WithMany(u => u.Ogloszenia)
                .HasForeignKey(o => o.AutorId);

            builder.Entity<Listing>()
                .HasOne(o => o.Kategoria)
                .WithMany(k => k.Ogloszenia)
                .HasForeignKey(o => o.KategoriaId);

            builder.Entity<Listing>()
                .HasOne(o => o.Lokalizacja)
                .WithMany(l => l.Ogloszenia)
                .HasForeignKey(o => o.LokalizacjaId);

            builder.Entity<Answer>()
                .HasOne(o => o.Listing)
                .WithMany(og => og.Odpowiedzi)
                .HasForeignKey(o => o.OgloszenieId);

            builder.Entity<Answer>()
                .HasOne(o => o.Autor)
                .WithMany(u => u.Odpowiedzi)
                .HasForeignKey(o => o.AutorId);

            builder.Entity<Photo>()
                .HasOne(z => z.Listing)
                .WithMany(o => o.Zdjecia)
                .HasForeignKey(z => z.OgloszenieId);

            // Indeksy dla lepszej wydajności
            builder.Entity<Listing>()
                .HasIndex(o => o.DataPublikacji);

            builder.Entity<Listing>()
                .HasIndex(o => new { o.KategoriaId, o.LokalizacjaId });
        }
    }
}