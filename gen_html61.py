import base64
import os
html = '<html><body>'
for i in range(16):
    path = f'frames61/frame_{i}.jpg'
    if os.path.exists(path):
        with open(path, 'rb') as f:
            encoded = base64.b64encode(f.read()).decode()
        html += f'<h2>Frame {i}</h2><img src="data:image/jpeg;base64,{encoded}" /><br>'
html += '</body></html>'
with open('frames61.html', 'w') as f:
    f.write(html)
