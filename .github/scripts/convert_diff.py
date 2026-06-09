#!/usr/bin/env python3
import re, sys

HUNK_RE = re.compile(r'^@@ -(\d+)(?:,\d+)? \+(\d+)(?:,\d+)? @@(.*)$')

lines = sys.stdin.read().splitlines()
output = ['# PR_DIFF_V1']
current_file = None
old_line = None
new_line = None
in_hunk = False

for line in lines:
    if line.startswith('diff --git '):
        if current_file is not None:
            output.append('END_FILE')
            output.append('')
        current_file = None
        in_hunk = False
        continue

    if not in_hunk and line.startswith('--- '):
        continue

    if not in_hunk and line.startswith('+++ '):
        path = line[4:].split('\t')[0]
        if path.startswith('b/'):
            path = path[2:]
        if path and path != '/dev/null':
            if current_file is not None:
                output.append('END_FILE')
                output.append('')
            output.append(f'FILE {path}')
            current_file = path
        continue

    hunk = HUNK_RE.match(line)
    if hunk and current_file:
        old_line = int(hunk.group(1))
        new_line = int(hunk.group(2))
        output.append(f'HUNK {line}')
        in_hunk = True
        continue

    if not in_hunk or current_file is None:
        continue
    if line.startswith('\\ No newline'):
        continue
    if old_line is None or new_line is None:
        continue

    marker = line[:1]
    content = line[1:]
    if marker == ' ':
        output.append(f'BOTH  {new_line:>4} | {content}')
        old_line += 1
        new_line += 1
    elif marker == '-':
        output.append(f'LEFT  {old_line:>4} | {content}')
        old_line += 1
    elif marker == '+':
        output.append(f'RIGHT {new_line:>4} | {content}')
        new_line += 1

if current_file is not None:
    output.append('END_FILE')

sys.stdout.write('\n'.join(output) + '\n')
