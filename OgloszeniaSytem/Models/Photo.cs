using System.ComponentModel.DataAnnotations;

namespace OgloszeniaSytem.Models
{
    public class Photo
    {
        public int Id { get; set; }
        
        [Required]
        public string NazwaPliku { get; set; } = string.Empty;
        
        public string? AltText { get; set; }
        public long RozmiarPliku { get; set; }
        public DateTime DataDodania { get; set; } = DateTime.UtcNow;
        
        // Klucz obcy
        public int OgloszenieId { get; set; }
        
        // Relacja
        public virtual Listing Listing { get; set; } = null!;
    }
}