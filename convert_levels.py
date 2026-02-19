
import json
import re
import os

file_path = "/Users/hierifer/rfv/Assets/Resources/mazeLevels.json"
with open(file_path, "r", encoding='utf-8') as f:
    content = f.read()

def parse_js_object(text):
    # Remove JS comments
    text = re.sub(r"//.*", "", text)
    # Remove trailing commas
    text = re.sub(r",\s*}", "}", text)
    text = re.sub(r",\s*]", "]", text)
    # Add quotes to unquoted keys
    # Match: { key: or , key: 
    # Be careful not to match inside strings. But here we assume simple structure.
    # Regex: Look for word followed by colon, preceded by { or , or newline
    # A bit risky but let's try.
    text = re.sub(r"([{,]\s*)([a-zA-Z0-9_]+)\s*:", r'\1"\2":', text)
    # Handle single quotes to double quotes for strings
    text = text.replace("'", '"')
    return json.loads(text)

# We use regex to find "export const levelN =" and capture N
# Then we find the first { and the matching }.

pattern = re.compile(r"export\s+const\s+level(\d+)\s*=")
matches = list(pattern.finditer(content))

final_levels = {}

for i, match in enumerate(matches):
    lvl_num = int(match.group(1))
    start_pos = match.end()
    
    # Analyze content starting from start_pos
    # We need to find the FIRST "{"
    fragment = content[start_pos:]
    start_brace = fragment.find("{")
    
    if start_brace == -1:
        print(f"Skipping level {lvl_num}: No starting brace found")
        continue

    # Now find matching brace
    brace_cnt = 0
    end_index = -1
    
    # We iterate char by char from start_brace
    for j, char in enumerate(fragment[start_brace:]):
        if char == "{":
            brace_cnt += 1
        elif char == "}":
            brace_cnt -= 1
            if brace_cnt == 0:
                end_index = start_brace + j + 1
                break
    
    if end_index != -1:
        json_str = fragment[start_brace:end_index]
        try:
            parsed = parse_js_object(json_str)
            final_levels[f"level{lvl_num}"] = parsed
        except Exception as e:
            print(f"Error parsing level {lvl_num}: {e}")
            # print(f"Content: {json_str}")

# Sort by level number
sorted_levels = dict(sorted(final_levels.items(), key=lambda item: int(item[0].replace("level", ""))))

output_path = "/Users/hierifer/rfv/Assets/Resources/mazeLevels.json"
with open(output_path, "w", encoding='utf-8') as f:
    json.dump(sorted_levels, f, indent=2, ensure_ascii=False)

print(f"Successfully converted {len(sorted_levels)} levels to {output_path}")
