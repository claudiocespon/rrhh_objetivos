import re

with open("scratch/navigate_and_generate.py", "r", encoding="utf-8") as f:
    lines = f.readlines()

for idx, line in enumerate(lines):
    if "add_body_text" in line:
        # Check if the line has 'doc,' or if the next line has it (in case of multiline call)
        # Let's inspect the context of the call
        context = "".join(lines[idx:idx+4])
        # Find if 'doc' is passed as the first parameter
        match = re.search(r'add_body_text\s*\(\s*([^,\n\)]*)', context)
        if match:
            first_arg = match.group(1).strip()
            if first_arg != "doc":
                print(f"Line {idx+1}: {line.strip()}")
                print(f"  First arg is '{first_arg}', expected 'doc'")
