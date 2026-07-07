import sqlite3
import hashlib
import os
import base64

iterations = 100000
salt = os.urandom(16)
hash_bytes = hashlib.pbkdf2_hmac('sha256', b'18', salt, iterations, dklen=32)
hash_str = f"{iterations}.{base64.b64encode(salt).decode('utf-8')}.{base64.b64encode(hash_bytes).decode('utf-8')}"

conn = sqlite3.connect(r'C:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
c = conn.cursor()
c.execute("UPDATE Usuarios SET PasswordHash = ?, DebeCambiarPassword = 0 WHERE Email = 'ptripodi@permaquim.com'", (hash_str,))
conn.commit()
conn.close()
print("Password updated to '18'")
