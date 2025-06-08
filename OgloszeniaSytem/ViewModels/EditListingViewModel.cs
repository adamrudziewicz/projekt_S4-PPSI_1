using System.ComponentModel.DataAnnotations;
using OgloszeniaSytem.Models;

namespace OgloszeniaSytem.ViewModels
{
    public class EditListingViewModel
    {
        public int Id { get; set; }

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

        public bool CzyAktywne { get; set; } = true;

        // Nowe zdjęcia do dodania
        public List<IFormFile>? NoweZdjecia { get; set; }

        // Istniejące zdjęcia
        public List<Photo> IstniejaceZdjecia { get; set; } = new List<Photo>();

        // ID zdjęć do usunięcia
        public List<int>? ZdjeciaDoUsuniecia { get; set; }
    }
}