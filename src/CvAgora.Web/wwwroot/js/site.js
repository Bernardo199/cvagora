// Smooth scroll para links internos
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function(e) {
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            e.preventDefault();
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    });
});

// Fechar menu mobile ao clicar num link
document.querySelectorAll('.nav ul a').forEach(link => {
    link.addEventListener('click', () => {
        document.querySelector('.nav ul')?.classList.remove('open');
    });
});

// Intersection Observer para animar cards ao entrar no viewport
if ('IntersectionObserver' in window) {
    const cards = document.querySelectorAll('.card, .cult-item');
    const obs = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
                obs.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    cards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
        obs.observe(card);
    });
}
