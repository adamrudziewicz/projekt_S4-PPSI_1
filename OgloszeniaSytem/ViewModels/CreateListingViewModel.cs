using System.ComponentModel.DataAnnotations;

namespace OgloszeniaSytem.ViewModels
{
    public class CreateListingViewModel
    {
        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [StringLength(200, ErrorMessage = "Tytuł może mieć maksymalnie 200 znaków")]
        public string Tytul { get; set; } = string.Empty;

        [Required(ErrorMessage = "Opis jest wymagany")]
        [StringLength(5000, ErrorMessage = "Opis może mieć maksymalnie 5000 znaków")]
        public string Opis { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Cena musi być dodatnia")]
        public decimal? Cena { get; set; }

        [Required(ErrorMessage = "Kategoria jest wymagana")]
        public int KategoriaId { get; set; }

        [Required(ErrorMessage = "Lokalizacja jest wymagana")]
        public int LokalizacjaId { get; set; }

        public DateTime? DataWaznosci { get; set; }

        public List<IFormFile>? Zdjecia { get; set; }
    }
}