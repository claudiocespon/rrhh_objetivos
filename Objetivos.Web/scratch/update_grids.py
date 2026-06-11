import os
import re

components_dir = r"c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\Components"

def update_grids():
    for root, dirs, files in os.walk(components_dir):
        for file in files:
            if file.endswith(".razor"):
                filepath = os.path.join(root, file)
                with open(filepath, "r", encoding="utf-8") as f:
                    content = f.read()
                
                # Check if it has a RadzenDataGrid
                if "RadzenDataGrid" not in content:
                    continue
                
                # Use regex to find <RadzenDataGrid ... > that don't have FilterCaseSensitivity
                # It might span multiple lines!
                pattern = re.compile(r'(<RadzenDataGrid\b[^>]*?)>', re.IGNORECASE | re.DOTALL)
                
                new_content = content
                matches = pattern.finditer(content)
                modified = False
                
                for match in matches:
                    tag_content = match.group(1)
                    if "FilterCaseSensitivity" not in tag_content:
                        # Append the property before the closing >
                        new_tag = tag_content + ' FilterCaseSensitivity="FilterCaseSensitivity.CaseInsensitive">'
                        new_content = new_content.replace(match.group(0), new_tag)
                        modified = True
                
                if modified:
                    with open(filepath, "w", encoding="utf-8") as f:
                        f.write(new_content)
                    print(f"Updated {filepath}")

if __name__ == "__main__":
    update_grids()
