import pandas as pd

df = pd.read_excel(r'c:\Development\Antigravity\RRHH_Objetivos\Planilla Final Nomina Regional Finalizada 3.xlsx')
print(df.columns.tolist())
print(df.head(5))
