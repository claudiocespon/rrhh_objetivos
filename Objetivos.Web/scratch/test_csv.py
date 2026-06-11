import csv
import unicodedata

def normalize(text):
    if not text or text == '-':
        return ""
    text = unicodedata.normalize('NFKD', text).encode('ASCII', 'ignore').decode('utf-8')
    return text.lower().replace('\xa0', ' ').strip()

with open(r'c:\Development\Antigravity\RRHH_Objetivos\usuarios_responsables.csv', 'r', encoding='utf-8-sig') as f:
    reader = csv.DictReader(f, delimiter=';')
    count = 0
    for row in reader:
        email = row.get('Email', '')
        resp = row.get('Responsable', '')
        if resp and resp != '-':
            count += 1
            if count < 5:
                print(f"{email} -> {resp} | normalized: {normalize(resp)}")
    print("Total with responsable:", count)
