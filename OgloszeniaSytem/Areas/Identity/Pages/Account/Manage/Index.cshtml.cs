using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OgloszeniaSytem.Models;

namespace OgloszeniaSytem.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string? Username { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Display(Name = "Imię")]
            [StringLength(50, ErrorMessage = "Imię nie może być dłuższe niż 50 znaków")]
            public string? Imie { get; set; }

            [Display(Name = "Nazwisko")]
            [StringLength(50, ErrorMessage = "Nazwisko nie może być dłuższe niż 50 znaków")]
            public string? Nazwisko { get; set; }

            [Phone]
            [Display(Name = "Numer telefonu")]
            public string? PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Imie = user.Imie,
                Nazwisko = user.Nazwisko,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Wystąpił błąd podczas aktualizacji numeru telefonu.";
                    return RedirectToPage();
                }
            }

            // Aktualizacja imienia i nazwiska
            if (user.Imie != Input.Imie || user.Nazwisko != Input.Nazwisko)
            {
                user.Imie = string.IsNullOrWhiteSpace(Input.Imie) ? null : Input.Imie.Trim();
                user.Nazwisko = string.IsNullOrWhiteSpace(Input.Nazwisko) ? null : Input.Nazwisko.Trim();
                
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Wystąpił błąd podczas aktualizacji profilu.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Twój profil został zaktualizowany";
            return RedirectToPage();
        }
    }
}