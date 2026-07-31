const fs = require('fs');

let html = fs.readFileSync('Views/All_Design_Components/Index.cshtml', 'utf8');

// 1. Remove global code buttons from headers
html = html.replace(/<button class="btn-edit text-xs px-3 py-1\.5 rounded-lg" onclick="toggleCode\('[^']+'\)">\s*<i class="fas fa-code"><\/i> Code\s*<\/button>/g, '');

// 2. Change the toggleCode function in script to use ID instead of class if it starts with #
// Wait, we can just change toggleCode to accept an ID.
html = html.replace(
    /window\.toggleCode = function\(cls\) { \$\('\.' \+ cls\)\.toggleClass\('show'\); };/,
    `window.toggleCode = function(id) { $('#' + id).toggleClass('show'); };`
);

// We need to write custom replacements for each section to split their code blocks.
// I will just let the LLM do it manually for the complex splits, but for buttons it's easy:
html = html.replace(
    /<h4 class="text-xs font-bold uppercase tracking-wider text-gray-400 mb-3">(.*?)<\/h4>\s*<div class="(.*?)">\s*([\s\S]*?)<\/div>\s*<div class="code-preview-container code-buttons mt-4">/g,
    (match, title, divCls, divContent) => {
        let id = 'code-btn-' + title.split('.')[0].trim();
        return `<div class="flex items-center justify-between mb-3">
    <h4 class="text-xs font-bold uppercase tracking-wider text-gray-400">\${title}</h4>
    <button class="text-[10px] bg-[var(--bg)] border border-[var(--border)] hover:bg-[var(--sidebar-hover-bg)] text-[var(--text-muted)] px-2.5 py-1 rounded-md transition" onclick="toggleCode('\${id}')"><i class="fas fa-code"></i> Code</button>
</div>
<div class="\${divCls}">
    \${divContent}
</div>
<div id="\${id}" class="code-preview-container mt-4">`;
    }
);

fs.writeFileSync('Views/All_Design_Components/Index.cshtml', html);
console.log('Done');
