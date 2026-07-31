const fs = require('fs');

let html = fs.readFileSync('Views/All_Design_Components/Index.cshtml', 'utf8');

// Cards replacements
html = html.replace(/bg-white dark:bg-gray-800/g, 'bg-[var(--surface)]');
html = html.replace(/border-gray-200 dark:border-gray-700/g, 'border-[var(--border)]');
html = html.replace(/text-gray-900 dark:text-white/g, 'text-[var(--text)]');
html = html.replace(/text-gray-500 dark:text-gray-400/g, 'text-[var(--text-muted)]');
html = html.replace(/text-gray-600 dark:text-gray-400/g, 'text-[var(--text-muted)]');

// Icon backgrounds in dark theme
html = html.replace(/bg-cyan-100 dark:bg-cyan-900\/30/g, 'bg-[rgba(var(--primary-rgb),0.1)]');
html = html.replace(/text-cyan-600 dark:text-cyan-400/g, 'text-[var(--primary)]');
html = html.replace(/hover:bg-cyan-200/g, 'hover:bg-[rgba(var(--primary-rgb),0.2)]');

fs.writeFileSync('Views/All_Design_Components/Index.cshtml', html);
console.log('Fixed cards');
