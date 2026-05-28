with open("scratch/navigate_and_generate.py", "r", encoding="utf-8") as f:
    lines = f.readlines()

new_lines = []
skip = 0
for idx, line in enumerate(lines):
    if skip > 0:
        skip -= 1
        continue
    
    # Check if this line is an add_body_text call
    if "add_body_text(" in line and "def add_body_text(" not in line:
        # Check if the next line already starts with the doc argument
        next_line = lines[idx + 1] if idx + 1 < len(lines) else ""
        if "doc," not in next_line and "doc ," not in next_line:
            # We need to insert doc, on the next line
            new_lines.append(line)
            # Find indentation of the next line
            indent = len(next_line) - len(next_line.lstrip())
            new_lines.append(" " * indent + "doc,\n")
            print(f"Fixed call at line {idx+1}")
            continue
            
    new_lines.append(line)

with open("scratch/navigate_and_generate.py", "w", encoding="utf-8") as f:
    f.writelines(new_lines)

print("Finished fixing add_body_text calls!")
