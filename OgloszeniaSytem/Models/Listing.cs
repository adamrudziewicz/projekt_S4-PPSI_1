using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgloszeniaSytem.Models
{
    public class Listing
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [StringLength(200, ErrorMessage = "Tytuł może mieć maksymalnie 200 znaków")]
        public string Tytul { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Opis jest wymagany")]
        [StringLength(5000, ErrorMessage = "Opis może mieć maksymalnie 5000 znaków")]
        public string Opis { get; set; } = string.Empty;
        
        [Range(0, double.MaxValue, ErrorMessage = "Cena musi być dodatnia")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Cena { get; set; }
        
        public DateTime DataPublikacji { get; set; } = DateTime.UtcNow;
        public DateTime? DataWaznosci { get; set; }
        public bool CzyAktywne { get; set; } = true;
        public int LiczbaWyswietlen { get; set; } = 0;
        
        // Klucze obce
        [Required(ErrorMessage = "Autor jest wymagany")]
        public string AutorId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Kategoria jest wymagana")]
        public int KategoriaId { get; set; }
        
        [Required(ErrorMessage = "Lokalizacja jest wymagana")]
        public int LokalizacjaId { get; set; }
        
        // Relacje - z bezpieczną inicjalizacją
        public virtual ApplicationUser? Autor { get; set; }
        public virtual Category? Kategoria { get; set; }
        public virtual Location? Lokalizacja { get; set; }
        public virtual ICollection<Answer> Odpowiedzi { get; set; } = new List<Answer>();
        public virtual ICollection<Photo> Zdjecia { get; set; } = new List<Photo>();
    }
}