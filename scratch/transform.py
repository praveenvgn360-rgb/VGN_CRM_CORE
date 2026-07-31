import re

def process_file():
    with open('Views/All_Design_Components/Index.cshtml', 'r', encoding='utf-8') as f:
        html = f.read()

    # 1. Remove global code buttons
    html = re.sub(
        r'<button class="btn-edit text-xs px-3 py-1\.5 rounded-lg" onclick="toggleCode\(\'[^\']+\'\)">\s*<i class="fas fa-code"></i> Code\s*</button>',
        '',
        html
    )

    # 2. Change toggleCode logic
    html = re.sub(
        r"window\.toggleCode = function\(cls\) \{ \$\('\.' \+ cls\)\.toggleClass\('show'\); \};",
        r"window.toggleCode = function(id) { $('#' + id).toggleClass('show'); };",
        html
    )

    # 3. For Buttons, they already have code blocks but we need to inject the toggle button
    # The pattern matches: <h4 class="...">Title</h4> ... <div class="code-preview-container code-buttons mt-4">
    def repl_btn(m):
        title = m.group(1)
        content = m.group(2)
        idx = title.split('.')[0].strip()
        id_str = f'code-btn-{idx}'
        return f'''<div class="flex items-center justify-between mb-3">
                            <h4 class="text-xs font-bold uppercase tracking-wider text-gray-400">{title}</h4>
                            <button class="text-[10px] bg-[var(--bg)] border border-[var(--border)] hover:bg-[var(--sidebar-hover-bg)] text-[var(--text-muted)] px-2.5 py-1 rounded-md transition" onclick="toggleCode('{id_str}')"><i class="fas fa-code"></i> Code</button>
                        </div>
                        <div class="flex flex-wrap gap-3">
                            {content}
                        </div>
                        <div id="{id_str}" class="code-preview-container mt-4">'''
    
    html = re.sub(
        r'<h4 class="text-xs font-bold uppercase tracking-wider text-gray-400 mb-3">(.*?)</h4>\s*<div class="flex flex-wrap gap-3">\s*(.*?)\s*</div>\s*<div class="code-preview-container code-buttons mt-4">',
        repl_btn,
        html,
        flags=re.DOTALL
    )

    with open('Views/All_Design_Components/Index.cshtml', 'w', encoding='utf-8') as f:
        f.write(html)

if __name__ == '__main__':
    process_file()
    print("Done refactoring buttons")
