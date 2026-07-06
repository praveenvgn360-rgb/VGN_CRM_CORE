/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Views/**/*.cshtml',
    './wwwroot/js/**/*.js',
    './Pages/**/*.cshtml',
    './Areas/**/*.cshtml'
  ],
  theme: {
    extend: {
      colors: {
        primary: 'var(--primary)',
        bg: 'var(--bg)',
        surface: 'var(--surface)',
        text: 'var(--text)',
        textMuted: 'var(--text-muted)',
        border: 'var(--border)'
      }
    },
  },
  plugins: [],
}
