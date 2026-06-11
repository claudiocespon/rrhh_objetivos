import json

def run():
    with open(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\scratch\db_backup.json', 'r', encoding='utf-8') as f:
        d = json.load(f)

    e_emails = {e['Email']: e for e in d['empleados']}
    j_emails = {j['Email']: j for j in d['jefes']}
    
    both = set(e_emails.keys()) & set(j_emails.keys())
    print('Overlaps:', len(both))
    for k in both:
        print(f'{k} (Emp: {e_emails[k]["Id"]}, Jefe: {j_emails[k]["Id"]})')

if __name__ == '__main__':
    run()
