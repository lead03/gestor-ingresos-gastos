/**
 * navbar.js — Lógica del menú hamburguesa (mobile)
 */
(function () {
    const toggle    = document.getElementById('navbar-toggle');
    const mobile    = document.getElementById('navbar-mobile');
    const iconMenu  = document.getElementById('navbar-icon-menu');
    const iconClose = document.getElementById('navbar-icon-close');

    if (!toggle || !mobile) return;

    function openMenu() {
        mobile.classList.add('navbar__mobile--open');
        iconMenu.style.display  = 'none';
        iconClose.style.display = 'block';
        toggle.setAttribute('aria-expanded', 'true');
        mobile.setAttribute('aria-hidden', 'false');
    }

    function closeMenu() {
        mobile.classList.remove('navbar__mobile--open');
        iconMenu.style.display  = 'block';
        iconClose.style.display = 'none';
        toggle.setAttribute('aria-expanded', 'false');
        mobile.setAttribute('aria-hidden', 'true');
    }

    toggle.addEventListener('click', function () {
        mobile.classList.contains('navbar__mobile--open') ? closeMenu() : openMenu();
    });

    // Cerrar al hacer clic en un enlace
    mobile.querySelectorAll('.navbar__mobile-link').forEach(function (link) {
        link.addEventListener('click', closeMenu);
    });

    // Cerrar al presionar Escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeMenu();
    });
})();
