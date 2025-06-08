// Asynchroniczne ładowanie odpowiedzi
$(document).ready(function() {
    // Dodawanie odpowiedzi AJAX
    $('#odpowiedzForm').on('submit', function(e) {
        e.preventDefault();
        
        const form = $(this);
        const ogloszenieId = $('#OgloszenieId').val();
        const tresc = $('#TrescOdpowiedzi').val().trim();
        
        if (!tresc) {
            alert('Wprowadź treść odpowiedzi');
            return;
        }
        
        // Wyłącz przycisk podczas wysyłania
        const submitBtn = form.find('button[type="submit"]');
        submitBtn.prop('disabled', true).text('Dodawanie...');
        
        $.ajax({
            url: '/Listing/AddResponse',
            type: 'POST',
            data: {
                ogloszenieId: ogloszenieId,
                tresc: tresc,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(data) {
                // Dodaj nową odpowiedź do listy
                $('#odpowiedziList').append(data);
                
                // Wyczyść formularz
                $('#TrescOdpowiedzi').val('');
                
                // Pokaż komunikat sukcesu
                showNotification('Odpowiedź została dodana!', 'success');
                
                // Przewiń do nowej odpowiedzi
                $('html, body').animate({
                    scrollTop: $('#odpowiedziList').offset().top
                }, 500);
            },
            error: function() {
                showNotification('Wystąpił błąd podczas dodawania odpowiedzi', 'error');
            },
            complete: function() {
                // Przywróć przycisk
                submitBtn.prop('disabled', false).text('Dodaj odpowiedź');
            }
        });
    });
    
    // Automatyczne sugestie miast
    $('#LokalizacjaInput').on('input', function() {
        const query = $(this).val();
        if (query.length >= 2) {
            $.ajax({
                url: '/api/Api/cities/suggestions', // Poprawny URL
                type: 'GET',
                data: { query: query },
                success: function(data) {
                    const suggestions = $('#citySuggestions');
                    suggestions.empty();

                    if (data.length > 0) {
                        suggestions.show();
                        data.forEach(function(city) {
                            suggestions.append(`<div class="suggestion-item" data-city="${city}">${city}</div>`);
                        });
                    } else {
                        suggestions.hide();
                    }
                },
                error: function(xhr, status, error) {
                    console.error('Błąd podczas ładowania sugestii miast:', xhr.responseText);
                }
            });
        }
    });
    
    // Kliknięcie w sugestię miasta
    $(document).on('click', '.suggestion-item', function() {
        const city = $(this).data('city');
        $('#LokalizacjaInput').val(city);
        $('#citySuggestions').hide();
    });
    
    // Walidacja formularza w czasie rzeczywistym
    $('form').on('submit', function(e) {
        const requiredFields = $(this).find('[required]');
        let isValid = true;
        
        requiredFields.each(function() {
            const field = $(this);
            if (!field.val().trim()) {
                field.addClass('is-invalid');
                isValid = false;
            } else {
                field.removeClass('is-invalid');
            }
        });
        
        if (!isValid) {
            e.preventDefault();
            showNotification('Wypełnij wszystkie wymagane pola', 'error');
        }
    });
    
    // Podgląd zdjęć przed uploadem
    $('#ZdjeciaInput').on('change', function() {
        const files = this.files;
        const preview = $('#zdjeciaPreview');
        preview.empty();
        
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            if (file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    preview.append(`
                        <div class="col-md-3 mb-2">
                            <img src="${e.target.result}" class="img-thumbnail" style="height: 100px; object-fit: cover;">
                        </div>
                    `);
                };
                reader.readAsDataURL(file);
            }
        }
    });
    
    // Automatyczne odświeżanie pogody
    loadWeatherWidget();
    setInterval(loadWeatherWidget, 300000); // co 5 minut
});

// Funkcje pomocnicze
function showNotification(message, type) {
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const notification = $(`
        <div class="alert ${alertClass} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999;">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);
    
    $('body').append(notification);
    
    // Automatyczne usunięcie po 5 sekundach
    setTimeout(function() {
        notification.alert('close');
    }, 5000);
}

function loadWeatherWidget() {
    const city = $('#weatherWidget').data('city') || 'Warszawa';

    console.log(`Ładowanie pogody dla miasta: ${city}`); // Debug

    $.ajax({
        url: `/api/Api/weather/${encodeURIComponent(city)}`, // Poprawny URL
        type: 'GET',
        success: function(data) {
            console.log('Dane pogodowe otrzymane:', data); // Debug

            if (data) {
                $('#weatherWidget').html(`
                    <div class="weather-info text-center">
                        <div class="mb-2">
                            <i class="fas fa-thermometer-half text-primary"></i>
                            <span class="fs-4 fw-bold">${Math.round(data.temperature)}°C</span>
                        </div>
                        <div class="mb-2">
                            <small class="text-muted">Odczuwalna: ${Math.round(data.feelsLike)}°C</small>
                        </div>
                        <div class="mb-2">
                            <span class="badge bg-light text-dark">${data.description}</span>
                        </div>
                        <div class="d-flex justify-content-around small text-muted">
                            <span><i class="fas fa-tint"></i> ${data.humidity}%</span>
                            <span><i class="fas fa-eye"></i> ${data.pressure} hPa</span>
                        </div>
                    </div>
                `);
            }
        },
        error: function(xhr, status, error) {
            console.error('Błąd podczas ładowania pogody:', xhr.responseText); // Debug
            $('#weatherWidget').html(`
                <div class="text-center text-muted">
                    <i class="fas fa-exclamation-triangle"></i>
                    <p class="mb-0">Brak danych pogodowych</p>
                    <small>Błąd: ${xhr.status}</small>
                </div>
            `);
        }
    });
}

// Smooth scrolling dla anchorów
$('a[href^="#"]').on('click', function(e) {
    e.preventDefault();
    const target = $(this.getAttribute('href'));
    if (target.length) {
        $('html, body').animate({
            scrollTop: target.offset().top - 80
        }, 500);
    }
});

// Lazy loading dla obrazków
document.addEventListener('DOMContentLoaded', function() {
    const images = document.querySelectorAll('img[data-src]');
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                observer.unobserve(img);
            }
        });
    });
    
    images.forEach(img => imageObserver.observe(img));
});