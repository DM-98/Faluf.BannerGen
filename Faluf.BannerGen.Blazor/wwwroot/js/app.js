window.app = {
    toggleTheme: () => {
        document.documentElement.classList.toggle('dark');
        localStorage.setItem('theme', document.documentElement.classList.contains('dark') ? 'dark' : 'light');
    },
    toast: (msg) => {
        const n = document.createElement('div');
        n.className = 'notification is-primary';
        n.innerText = msg;
        n.style.position = 'fixed'; n.style.top = '1rem'; n.style.right = '1rem';
        document.body.appendChild(n);
        setTimeout(() => n.remove(), 4000);
    }
};

document.addEventListener('DOMContentLoaded', () => {
    if (localStorage.getItem('theme') === 'dark') document.documentElement.classList.add('dark');
});