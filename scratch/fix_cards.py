import re

def process_file():
    with open('Views/All_Design_Components/Index.cshtml', 'r', encoding='utf-8') as f:
        html = f.read()

    # Cards replacements
    html = html.replace('bg-white dark:bg-gray-800', 'bg-[var(--surface)]')
    html = html.replace('border-gray-200 dark:border-gray-700', 'border-[var(--border)]')
    html = html.replace('text-gray-900 dark:text-white', 'text-[var(--text)]')
    html = html.replace('text-gray-500 dark:text-gray-400', 'text-[var(--text-muted)]')
    html = html.replace('text-gray-600 dark:text-gray-400', 'text-[var(--text-muted)]')

    # Icon backgrounds in dark theme
    html = html.replace('bg-cyan-100 dark:bg-cyan-900/30', 'bg-[rgba(var(--primary-rgb),0.1)]')
    html = html.replace('text-cyan-600 dark:text-cyan-400', 'text-[var(--primary)]')
    html = html.replace('hover:bg-cyan-200', 'hover:bg-[rgba(var(--primary-rgb),0.2)]')

    # Same for emerald/purple/amber
    html = html.replace('bg-emerald-100 dark:bg-emerald-900/30', 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600 dark:text-emerald-400') 
    # Wait, the best way for these static colors is to just use standard tailwind. 
    # If they use 'bg-emerald-100 dark:bg-emerald-900/30', it's fine as long as `dark:` is fixed in tailwind.config!
    
    with open('Views/All_Design_Components/Index.cshtml', 'w', encoding='utf-8') as f:
        f.write(html)

if __name__ == '__main__':
    process_file()
