import base64
import glob

files = glob.glob('c:/Development/Antigravity/RRHH_Objetivos/frame_*.jpg')
files.sort()

html = '<html><body>'
for f in files:
    with open(f, 'rb') as img:
        b64 = base64.b64encode(img.read()).decode()
        html += f'<h3>{f}</h3><img src="data:image/jpeg;base64,{b64}" style="max-width:800px;"/><br/>'
html += '</body></html>'

with open('check_frames.html', 'w') as out:
    out.write(html)
