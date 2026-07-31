const fs = require('fs');

let html = fs.readFileSync('Views/Shared/_Layout.cshtml', 'utf8');

// Change tailwind config
html = html.replace("darkMode: 'class',", "darkMode: ['class', '[data-theme=\"dark\"]'],");

fs.writeFileSync('Views/Shared/_Layout.cshtml', html);
console.log('Fixed config');
