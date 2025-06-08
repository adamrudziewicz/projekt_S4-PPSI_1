// Funkcja do ładowania podobnych ogłoszeń
function loadSimilarListings(listingId, containerId = 'similarListings') {
    console.log(`Ładowanie podobnych ogłoszeń dla ID: ${listingId}`);

    // Pokaż loader
    $(`#${containerId}`).html(`
        <div class="text-center py-3">
            <div class="spinner-border spinner-border-sm text-primary" role="status">
                <span class="visually-hidden">Ładowanie...</span>
            </div>
            <small class="d-block mt-2 text-muted">Szukam podobnych...</small>
        </div>
    `);

    $.ajax({
        url: `/api/Api/listing/similar/${listingId}?count=2`,
        type: 'GET',
        success: function(data) {
            console.log('Podobne ogłoszenia otrzymane:', data);
            displaySimilarListings(data, containerId);
        },
        error: function(xhr, status, error) {
            console.error('Błąd podczas ładowania podobnych ogłoszeń:', error);
            $(`#${containerId}`).html(`
                <small class="text-muted">Brak podobnych ogłoszeń</small>
            `);
        }
    });
}

function displaySimilarListings(listings, containerId) {
    const container = $(`#${containerId}`);

    if (!listings || listings.length === 0) {
        container.html(`
            <div class="text-center py-3">
                <i class="fas fa-search text-muted mb-2 d-block" style="font-size: 1.5rem;"></i>
                <small class="text-muted">Brak podobnych ogłoszeń</small>
            </div>
        `);
        return;
    }

    let html = '';

    listings.forEach((listing, index) => {
        const imageHtml = listing.pierwszeZdjecie
            ? `<img src="/uploads/${listing.pierwszeZdjecie}" class="similar-listing-img" alt="${listing.tytul}">`
            : `<div class="similar-listing-placeholder">
                 <i class="fas fa-image text-muted"></i>
               </div>`;

        const priceText = listing.cena
            ? `${parseFloat(listing.cena).toFixed(0)} zł`
            : 'Brak ceny';

        html += `
            <div class="similar-listing-item" onclick="window.location.href='/Listing/Details/${listing.id}'">
                <div class="d-flex">
                    <div class="similar-listing-image-container">
                        ${imageHtml}
                    </div>
                    <div class="flex-grow-1">
                        <h6 class="similar-listing-title" title="${listing.tytul}">
                            ${listing.tytul}
                        </h6>
                        <div class="similar-listing-badges mb-2">
                            <span class="badge bg-primary">${priceText}</span>
                            <span class="badge" style="background: var(--gradient-success); color: white;">${Math.round(listing.similarityScore)}% podobne</span>
                        </div>
                        <small class="text-muted d-block">
                            <i class="fas fa-map-marker-alt me-1"></i>${listing.lokalizacjaNazwa}
                        </small>
                    </div>
                </div>
            </div>
        `;
    });

    // Dodaj link do wszystkich podobnych ogłoszeń
    html += `
        <div class="similar-listings-footer">
            <small>
                <a href="/Listing?search=${encodeURIComponent(listings[0]?.kategoriaNazwa || '')}" class="text-decoration-none">
                    <i class="fas fa-arrow-right me-1"></i>Zobacz więcej podobnych
                </a>
            </small>
        </div>
    `;

    container.html(html);
}

// Automatyczne ładowanie
$(document).ready(function() {
    const listingId = $('#currentListingId').val() || $('[data-listing-id]').data('listing-id');
    if (listingId && $('#similarListings').length > 0) {
        loadSimilarListings(listingId);
    }
});