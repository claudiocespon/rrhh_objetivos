import sqlite3
import hashlib
import base64
import os

db_path = 'Objetivos.Web/objetivos.db'
conn = sqlite3.connect(db_path)
c = conn.cursor()

# Hash for Pablo (18)
salt1 = os.urandom(16)
hash1 = hashlib.pbkdf2_hmac('sha256', b'18', salt1, 100000, 32)
hash_str1 = f"100000.{base64.b64encode(salt1).decode('utf-8')}.{base64.b64encode(hash1).decode('utf-8')}"

# Hash for Nicolas (560)
salt2 = os.urandom(16)
hash2 = hashlib.pbkdf2_hmac('sha256', b'560', salt2, 100000, 32)
hash_str2 = f"100000.{base64.b64encode(salt2).decode('utf-8')}.{base64.b64encode(hash2).decode('utf-8')}"

c.execute("UPDATE Jefes SET PasswordHash = ?, DebeCambiarPassword = 0 WHERE Email = 'ptripodi@permaquim.com'", (hash_str1,))
c.execute("UPDATE Empleados SET PasswordHash = ?, DebeCambiarPassword = 0 WHERE Email = 'ncaldiroli@permaquim.com'", (hash_str2,))

conn.commit()
print("Passwords updated in database successfully!")
conn.close()
