using Microsoft.AspNetCore.Identity;

namespace OgloszeniaSytem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Imie { get; set; }
        public string? Nazwisko { get; set; }
        public DateTime DataRejestracji { get; set; } = DateTime.UtcNow;
        
        // Relacje
        public virtual ICollection<Listing> Ogloszenia { get; set; } = new List<Listing>();
        public virtual ICollection<Answer> Odpowiedzi { get; set; } = new List<Answer>();
    }
}