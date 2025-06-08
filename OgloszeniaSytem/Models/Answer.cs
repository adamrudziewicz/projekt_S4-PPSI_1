using System.ComponentModel.DataAnnotations;

namespace OgloszeniaSytem.Models
{
    public class Answer
    {
        public int Id { get; set; }
        
        [Required]
        public string Tresc { get; set; } = string.Empty;
        
        public DateTime DataOdpowiedzi { get; set; } = DateTime.UtcNow;
        
        // Klucze obce
        public int OgloszenieId { get; set; }
        public string AutorId { get; set; } = string.Empty;
        
        // Relacje
        public virtual Listing Listing { get; set; } = null!;
        public virtual ApplicationUser Autor { get; set; } = null!;
    }
}