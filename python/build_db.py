"""Creates claims.db from the SQL scripts, loads seeded synthetic data, exports CSVs for Power BI and the .NET API."""
import csv, random, sqlite3, datetime as dt
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
DB = ROOT / "data" / "claims.db"
random.seed(42)
STATES = ["CA","TX","NY","FL","IL","WA","NJ","OH"]
TYPES = {"Auto":(1300,(300,2500)), "Home":(1500,(500,4500)), "Health":(3200,(150,2200)), "Life":(1800,(2000,12000))}

def main():
    DB.parent.mkdir(exist_ok=True)
    if DB.exists(): DB.unlink()
    con = sqlite3.connect(DB)
    con.executescript((ROOT/"sql"/"01_schema.sql").read_text())
    cust = [(i, f"Customer {i:04d}", random.choice(STATES), random.randint(1955, 2002)) for i in range(1, 501)]
    con.executemany("INSERT INTO customers VALUES (?,?,?,?)", cust)
    pols, pid = [], 1
    for c in cust:
        for _ in range(random.choice([1,1,2])):
            t = random.choice(list(TYPES))
            prem = round(TYPES[t][0] * random.uniform(0.8, 1.3), 2)
            start = dt.date(2023,1,1) + dt.timedelta(days=random.randint(0, 400))
            pols.append((pid, c[0], t, start.isoformat(), prem)); pid += 1
    con.executemany("INSERT INTO policies VALUES (?,?,?,?,?)", pols)
    claims, cid = [], 1
    for p in pols:
        for _ in range(random.choices([0,1,2,3],[72,20,6,2])[0]):
            lo, hi = TYPES[p[2]][1]
            amt = round(random.uniform(lo, hi), 2)
            if random.random() < 0.03: amt = round(hi * random.uniform(3.5, 5), 2)
            d = dt.date.fromisoformat(p[3]) + dt.timedelta(days=random.randint(30, 600))
            d = min(d, dt.date(2025,12,31))
            st = random.choices(["Approved","Rejected","Pending"],[70,20,10])[0]
            claims.append((cid, p[0], d.isoformat(), amt, st)); cid += 1
    con.executemany("INSERT INTO claims VALUES (?,?,?,?,?)", claims)
    con.executescript((ROOT/"sql"/"02_views.sql").read_text())
    con.commit()
    for view, name in [("vw_claims_enriched","claims_enriched.csv"), ("vw_fraud_flags","fraud_flags.csv")]:
        cur = con.execute(f"SELECT * FROM {view} ORDER BY claim_id")
        with open(ROOT/"data"/name, "w", newline="") as f:
            w = csv.writer(f); w.writerow([d[0] for d in cur.description]); w.writerows(cur)
    n = con.execute("SELECT COUNT(*) FROM claims").fetchone()[0]
    fl = con.execute("SELECT COUNT(*) FROM vw_fraud_flags WHERE high_amount_flag=1 OR repeat_claimant_flag=1").fetchone()[0]
    print(f"customers={len(cust)} policies={len(pols)} claims={n} flagged={fl}")

if __name__ == "__main__":
    main()
