import base64
with open('patch_header.png', 'rb') as f:
    header = base64.b64encode(f.read()).decode()
with open('patch_sidebar.png', 'rb') as f:
    sidebar = base64.b64encode(f.read()).decode()
html = f'<html><body><h2>Header</h2><img src="data:image/png;base64,{header}" /><h2>Sidebar</h2><img src="data:image/png;base64,{sidebar}" /></body></html>'
with open('patches.html', 'w') as f:
    f.write(html)
