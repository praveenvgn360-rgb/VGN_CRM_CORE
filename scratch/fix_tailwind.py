import re
import io

def process_file():
    with io.open('Views/Shared/_Layout.cshtml', 'r', encoding='utf-8') as f:
        html = f.read()

    # Change tailwind config
    html = html.replace("darkMode: 'class',", "darkMode: ['class', '[data-theme=\"dark\"]'],")

    with io.open('Views/Shared/_Layout.cshtml', 'w', encoding='utf-8') as f:
        f.write(html)

if __name__ == '__main__':
    process_file()
