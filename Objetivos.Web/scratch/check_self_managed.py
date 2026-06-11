import pandas as pd
import unicodedata

def normalize(text):
    if not isinstance(text, str):
        return ""
    if not text or text == '-' or text.lower() == 'no aplica':
        return ""
    text = unicodedata.normalize('NFKD', text).encode('ASCII', 'ignore').decode('utf-8')
    return text.lower().replace('\xa0', ' ').replace(',', ' ').strip()

df = pd.read_excel(r'c:\Development\Antigravity\RRHH_Objetivos\Planilla Final Nomina Regional Finalizada 3.xlsx', header=2)

for idx, row in df.iterrows():
    email = str(row.get('Mail', '')).strip()
    resp_str = str(row.get('Responsable de evaluación', '')).strip()
    nombre_apellido = str(row.get('Apellido\xa0y\xa0Nombre', '')).strip()
    
    if normalize(resp_str) == normalize(nombre_apellido):
        print(f"Self-managed: {email} ({nombre_apellido})")
