const fs = require('fs');

let html = fs.readFileSync('Views/All_Design_Components/Index.cshtml', 'utf8');

function processSection(sectionName, prefix, count) {
    // We will find the global code block and remove it
    let regexGlobal = new RegExp(`<div id="code-${sectionName}" class="code-preview-container">[\\s\\S]*?</div>\\s*</div>`, 'g');
    html = html.replace(regexGlobal, '</div>');

    for (let i = 1; i <= count; i++) {
        let regex = new RegExp(`<!-- ===== ${prefix} ${i}: (.*?) ===== -->[\\s\\S]*?<h4 class="text-xs font-bold uppercase tracking-wider text-gray-400 mb-3">.*?</h4>([\\s\\S]*?)(?=<!-- ===== ${prefix} ${i+1}:|</div>\\s*<!-- ===== CARD TYPE|</div>\\s*<div id="code-|</section>|</div>\\s*</div>\\s*</section>)`, 'g');
        // It's too complex to reliably parse HTML with regex for this structure.
    }
}

// Since Regex parsing HTML is dangerous, let's just use string splitting.
function refactorLayout() {
    let parts = html.split('<!-- ===== SECTION: LAYOUT & CARDS ===== -->');
    if (parts.length < 2) return;
    
    let layoutPart = parts[1].split('<!-- ===== SECTION: FLOWBITE COMPONENTS ===== -->')[0];
    
    // Remove global code preview from layoutPart
    layoutPart = layoutPart.replace(/<div id="code-layout" class="code-preview-container">[\s\S]*?<\/pre>\s*<\/div>/, '');

    // Now split layoutPart by "<!-- ===== CARD TYPE "
    let cards = layoutPart.split('<!-- ===== CARD TYPE ');
    for (let i = 1; i < cards.length; i++) {
        let card = cards[i];
        
        // Find the title
        let titleMatch = card.match(/<h4 class="text-xs font-bold uppercase tracking-wider text-gray-400 mb-3">(.*?)<\/h4>/);
        if (titleMatch) {
            let title = titleMatch[1];
            let id = 'code-card-' + i;
            
            // replace the title with title + button
            card = card.replace(titleMatch[0], `<div class="flex items-center justify-between mb-3">
                            <h4 class="text-xs font-bold uppercase tracking-wider text-gray-400">${title}</h4>
                            <button class="text-[10px] bg-[var(--bg)] border border-[var(--border)] hover:bg-[var(--sidebar-hover-bg)] text-[var(--text-muted)] px-2.5 py-1 rounded-md transition" onclick="toggleCode('${id}')"><i class="fas fa-code"></i> Code</button>
                        </div>`);
            
            // Find the end of the card div, wait, we can just append the code block at the end before the next section
            // Actually, we don't need to extract the exact code, we can just put a placeholder or extract the HTML
            // Let's just put the HTML of the first child inside the grid
            let contentMatch = card.match(/<div class="(?:grid|bg-\[var\(--surface\)|flex)[^>]*>([\s\S]*?)<!-- =====/);
            // Too complex to extract inner HTML reliably without DOM.
        }
    }
}

console.log('Script skipped to avoid breaking HTML');
