import sqlite3
import os

def run():
    db_path = r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db'
    sql_path = r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\update.sql'

    with open(sql_path, 'r', encoding='utf-8') as f:
        sql = f.read()

    # Remove the PRAGMA foreign_keys = 0 line since Python sqlite3 runs without them anyway
    sql = sql.replace('PRAGMA foreign_keys = 0;', '')

    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    try:
        cursor.executescript(sql)
        conn.commit()
        print("Migration applied successfully.")
    except Exception as e:
        print(f"Error applying migration: {e}")
        conn.rollback()
    finally:
        conn.close()

if __name__ == '__main__':
    run()
