import pandas as pd

df = pd.read_excel(r'c:\Development\Antigravity\RRHH_Objetivos\Planilla Final Nomina Regional Finalizada 3.xlsx', header=1)
print(df.columns.tolist())
print(df[['Nombre y Apellido', 'Email', 'Responsable Evaluacion Desempeo']].head(10))
