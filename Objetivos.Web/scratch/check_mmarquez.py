import sqlite3

c = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db').cursor()
c.execute("SELECT JefeId FROM Empleados WHERE Email='mmarquez@permaquim.com'")
print(c.fetchone())
