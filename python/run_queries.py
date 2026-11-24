"""Runs sql/03_analysis_queries.sql against claims.db and prints each result."""
import sqlite3, re
from pathlib import Path
ROOT = Path(__file__).resolve().parent.parent
con = sqlite3.connect(ROOT/"data"/"claims.db")
for block in re.split(r"(?m)^-- (?=Q\d)", (ROOT/"sql"/"03_analysis_queries.sql").read_text())[1:]:
    title, _, sql = block.partition("\n")
    cur = con.execute(sql)
    print(f"\n== {title} ==")
    print(" | ".join(d[0] for d in cur.description))
    for row in cur.fetchmany(8): print(" | ".join(str(x) for x in row))
