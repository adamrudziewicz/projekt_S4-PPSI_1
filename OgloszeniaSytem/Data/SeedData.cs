using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OgloszeniaSytem.Models;

namespace OgloszeniaSytem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Tworzenie ról
                string[] roleNames = { "Admin", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger.LogInformation("Utworzono rolę: {RoleName}", roleName);
                    }
                }

                // Tworzenie administratora
                var adminEmail = "admin@ogloszenia.pl";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        Imie = "Administrator",
                        Nazwisko = "Systemu",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Utworzono administratora: {Email}", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Błąd podczas tworzenia administratora: {Errors}", 
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // Seed kategorii
                if (!await context.Kategorie.AnyAsync())
                {
                    var kategorie = new[]
                    {
                        new Category { Nazwa = "Motoryzacja", Opis = "Samochody, motocykle, części", Ikona = "fas fa-car" },
                        new Category { Nazwa = "Nieruchomości", Opis = "Mieszkania, domy, działki", Ikona = "fas fa-home" },
                        new Category { Nazwa = "Praca", Opis = "Oferty pracy, CV", Ikona = "fas fa-briefcase" },
                        new Category { Nazwa = "Elektronika", Opis = "Telefony, komputery, RTV", Ikona = "fas fa-laptop" },
                        new Category { Nazwa = "Dom i Ogród", Opis = "Meble, dekoracje, narzędzia", Ikona = "fas fa-couch" },
                        new Category { Nazwa = "Moda", Opis = "Ubrania, buty, akcesoria", Ikona = "fas fa-tshirt" },
                        new Category { Nazwa = "Sport", Opis = "Sprzęt sportowy, fitness", Ikona = "fas fa-running" },
                        new Category { Nazwa = "Hobby", Opis = "Książki, muzyka, kolekcje", Ikona = "fas fa-guitar" }
                    };

                    await context.Kategorie.AddRangeAsync(kategorie);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Dodano {Count} kategorii", kategorie.Length);
                }

                // Seed lokalizacji
                if (!await context.Lokalizacje.AnyAsync())
                {
                    var lokalizacje = new[]
                    {
                        new Location { Nazwa = "Warszawa", Kod = "WAW", Wojewodztwo = "Mazowieckie" },
                        new Location { Nazwa = "Kraków", Kod = "KRK", Wojewodztwo = "Małopolskie" },
                        new Location { Nazwa = "Gdańsk", Kod = "GDA", Wojewodztwo = "Pomorskie" },
                        new Location { Nazwa = "Wrocław", Kod = "WRO", Wojewodztwo = "Dolnośląskie" },
                        new Location { Nazwa = "Poznań", Kod = "POZ", Wojewodztwo = "Wielkopolskie" },
                        new Location { Nazwa = "Łódź", Kod = "LOD", Wojewodztwo = "Łódzkie" }
                    };

                    await context.Lokalizacje.AddRangeAsync(lokalizacje);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Dodano {Count} lokalizacji", lokalizacje.Length);
                }

                // Przykładowe ogłoszenia (tylko jeśli admin istnieje i są kategorie/lokalizacje)
                if (!await context.Ogloszenia.AnyAsync() && adminUser != null)
                {
                    var kategoria = await context.Kategorie.FirstAsync();
                    var lokalizacja = await context.Lokalizacje.FirstAsync();

                    var przykladoweOgloszenia = new[]
                    {
                        new Listing
                        {
                            Tytul = "Sprzedam samochód Toyota Corolla 2015",
                            Opis = "Samochód w bardzo dobrym stanie, pierwszy właściciel, serwisowany w ASO. Przebieg 120 000 km.",
                            Cena = 35000.00m,
                            AutorId = adminUser.Id,
                            KategoriaId = kategoria.Id,
                            LokalizacjaId = lokalizacja.Id,
                            DataWaznosci = DateTime.UtcNow.AddDays(30),
                            CzyAktywne = true
                        },
                        new Listing
                        {
                            Tytul = "Laptop Dell - stan bardzo dobry",
                            Opis = "Laptop używany przez rok, wszystko działa sprawnie. Idealny do pracy biurowej.",
                            Cena = 2500.00m,
                            AutorId = adminUser.Id,
                            KategoriaId = kategoria.Id,
                            LokalizacjaId = lokalizacja.Id,
                            DataWaznosci = DateTime.UtcNow.AddDays(15),
                            CzyAktywne = true
                        }
                    };

                    await context.Ogloszenia.AddRangeAsync(przykladoweOgloszenia);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Dodano {Count} przykładowych ogłoszeń", przykladoweOgloszenia.Length);
                }

                logger.LogInformation("Inicjalizacja danych zakończona pomyślnie");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Błąd podczas inicjalizacji danych");
                throw;
            }
        }
    }
}