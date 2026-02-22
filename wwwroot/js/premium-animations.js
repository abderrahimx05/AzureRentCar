/* ===== PREMIUM UI - ANIMATIONS AVANCÉES ===== */

// Smooth scroll behavior
document.documentElement.style.scrollBehavior = 'smooth';

// ===== ANIMATED COUNTER =====
export function animateCounter(element, target, duration = 2000) {
    let start = 0;
    const increment = target / (duration / 16);
    
    function updateCount() {
        start += increment;
        if (start < target) {
            element.textContent = Math.ceil(start);
            requestAnimationFrame(updateCount);
        } else {
            element.textContent = target;
        }
    }
    
    updateCount();
}

// ===== PARALLAX EFFECT =====
export function enableParallax() {
    const parallaxElements = document.querySelectorAll('[data-parallax]');
    
    window.addEventListener('scroll', () => {
        parallaxElements.forEach(el => {
            const speed = el.dataset.parallax || 0.5;
            const yOffset = window.scrollY * speed;
            el.style.transform = `translateY(${yOffset}px)`;
        });
    });
}

// ===== REVEAL ON SCROLL =====
export function revealOnScroll() {
    const reveals = document.querySelectorAll('[data-reveal]');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('revealed');
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -100px 0px'
    });
    
    reveals.forEach(reveal => {
        observer.observe(reveal);
    });
}

// ===== COLOR SHIFT ON SCROLL =====
export function colorShiftOnScroll() {
    const hero = document.querySelector('.hero-section');
    if (!hero) return;
    
    window.addEventListener('scroll', () => {
        const scroll = window.scrollY;
        const maxScroll = window.innerHeight;
        const percentage = Math.min(scroll / maxScroll, 1);
        
        const hue = 200 + (percentage * 30); // From blue to cyan
        hero.style.filter = `hue-rotate(${hue - 200}deg)`;
    });
}

// ===== MOUSE FOLLOW EFFECT =====
export function mouseFollowEffect() {
    const cards = document.querySelectorAll('.feature-card, .car-card');
    
    cards.forEach(card => {
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            const centerX = rect.width / 2;
            const centerY = rect.height / 2;
            
            const rotateX = (y - centerY) * 0.1;
            const rotateY = (centerX - x) * 0.1;
            
            card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
        });
        
        card.addEventListener('mouseleave', () => {
            card.style.transform = 'perspective(1000px) rotateX(0) rotateY(0)';
        });
    });
}

// ===== INIT ALL ANIMATIONS =====
export function initAllAnimations() {
    enableParallax();
    revealOnScroll();
    colorShiftOnScroll();
    mouseFollowEffect();
}

// Auto-run on load
document.addEventListener('DOMContentLoaded', initAllAnimations);
