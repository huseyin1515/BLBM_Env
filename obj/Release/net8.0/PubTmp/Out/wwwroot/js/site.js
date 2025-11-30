$(document).ready(function () {
    // --- GÜNCELLENDİ: Eski #menu-toggle click fonksiyonu tamamen kaldırıldı. ---

    // Aktif menü elemanını vurgulama mantığı yeni navbar yapısına uyarlandı.
    $(function () {
        var currentPath = location.pathname;
        if (currentPath === '/') { currentPath = '/Envanter/Index'; }

        // Seçici '.list-group-item' yerine '.nav-link' olarak değiştirildi.
        $('.navbar-nav .nav-link').each(function () {
            var $this = $(this);
            // Linkin href'i mevcut yol ile başlıyorsa 'active' sınıfını ekle
            if (currentPath.toLowerCase().startsWith($this.attr('href').toLowerCase())) {
                $('.nav-link').removeClass('active');
                $this.addClass('active');
            }
        });
    });
});